/**
 * @module og/webgl/Handler
 */

'use strict';

import { cons } from '../cons.js';
import { Clock } from '../Clock.js';
import { ImageCanvas } from '../ImageCanvas.js';
import { isEmpty } from '../utils/shared.js';
import { ProgramController } from './ProgramController.js';
import { Stack } from '../Stack.js';
import { Vec2 } from '../math/Vec2.js';

/**
 * Maximum texture image size.
 * @const
 * @type {number}
 */
const MAX_SIZE = 4096;

const vendorPrefixes = ["", "WEBKIT_", "MOZ_"];

/**
 * A WebGL handler for accessing low-level WebGL capabilities.
 * @class
 * @param {string} id - Canvas element id that WebGL handler assing with. If it's null
 * or undefined creates hidden canvas and handler bacomes hidden.
 * @param {Object} [params] - Handler options:
 * @param {number} [params.anisotropy] - Anisitropy filter degree. 8 is default.
 * @param {number} [params.width] - Hidden handler width. 256 is default.
 * @param {number} [params.height] - Hidden handler height. 256 is default.
 * @param {Object} [param.scontext] - Native WebGL context attributes. See https://www.khronos.org/registry/webgl/specs/latest/1.0/#WEBGLCONTEXTATTRIBUTES
 * @param {Array.<string>} [params.extensions] - Additional WebGL extension list. Available by default: EXT_texture_filter_anisotropic.
 */
const Handler = function (id, params) {

    params = params || {};

    /**
     * Application default timer.
     * @public
     * @type {og.Clock}
     */
    this.defaultClock = new Clock();

    /**
     * Custom timers.
     * @protected
     * @type{og.Clock[]}
     */
    this._clocks = [];

    /**
     * Draw frame time in milliseconds.
     * @public
     * @readonly
     * @type {number}
     */
    this.deltaTime = 0;

    /**
     * WebGL rendering canvas element.
     * @public
     * @type {Object}
     */
    this.canvas = null;

    /**
     * WebGL context.
     * @public
     * @type {Object}
     */
    this.gl = null;

    /**
     * Shader program controller list.
     * @public
     * @type {Object.<og.webgl.ProgramController>}
     */
    this.programs = {};

    /**
     * Current active shader program controller.
     * @public
     * @type {og.webgl.ProgramController}
     */
    this.activeProgram = null;

    /**
     * Handler parameters.
     * @private
     * @type {Object}
     */
    this._params = params || {};
    this._params.anisotropy = this._params.anisotropy || 8;
    var w = this._params.width;
    if (w > MAX_SIZE) {
        w = MAX_SIZE;
    }
    this._params.width = w || 256;

    var h = this._params.height;
    if (h > MAX_SIZE) {
        h = MAX_SIZE;
    }
    this._params.height = h || 256;
    this._params.context = this._params.context || {};
    this._params.extensions = this._params.extensions || [];
    this._oneByHeight = 1 / this._params.height;

    /**
     * Current WebGL extensions. Becomes here after context initialization.
     * @public
     * @type {Object}
     */
    this.extensions = {};

    /**
     * HTML Canvas object id.
     * @private
     * @type {Object}
     */
    this._id = id;

    this._lastAnimationFrameTime = 0;

    this._initialized = false;

    /**
     * Animation frame function assigned from outside(Ex. from Renderer).
     * @private
     * @type {frameCallback}
     */
    this._frameCallback = function () { };

    this.transparentTexture = null;

    this.framebufferStack = new Stack();

    if (params.autoActivate || isEmpty(params.autoActivate)) {
        this.initialize();
    }
};

/**
 * The return value is null if the extension is not supported, or an extension object otherwise.
 * @param {Object} gl - WebGl context pointer.
 * @param {String} name - Extension name.
 * @returns {Object} -
 */
Handler.getExtension = function (gl, name) {
    var i, ext;
    for (i in vendorPrefixes) {
        ext = gl.getExtension(vendorPrefixes[i] + name);
        if (ext) {
            return ext;
        }
    }
    return null;
};

const CONTEXT_TYPE = ["webgl2", "webgl"];

/**
 * Returns a drawing context on the canvas, or null if the context identifier is not supported.
 * @param {Object} canvas - HTML canvas object.
 * @param {Object} [contextAttributes] - See canvas.getContext contextAttributes.
 * @returns {Object} -
 */
Handler.getContext = function (canvas, contextAttributes) {
    var ctx = null;

    try {
        for (let i = 0; i < CONTEXT_TYPE.length; i++) {
            ctx = canvas.getContext(CONTEXT_TYPE[i], contextAttributes);
            if (ctx) {
                ctx.type = CONTEXT_TYPE[i];
                break;
            }
        }
    } catch (ex) {
        cons.logErr("exception during the GL context initialization");
    }

    if (!ctx) {
        cons.logErr("could not initialise WebGL");
    }

    return ctx;
};

/**
 * Sets animation frame function.
 * @public
 * @param {callback} callback - Frame callback.
 */
Handler.prototype.setFrameCallback = function (callback) {
    callback && (this._frameCallback = callback);
};

/**
 * Creates NEAREST filter texture.
 * @public
 * @param {Object} image - Image or Canvas object.
 * @returns {Object} - WebGL texture object.
 */
Handler.prototype.createTexture_n = function (image) {
    var gl = this.gl;
    var texture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    gl.bindTexture(gl.TEXTURE_2D, null);
    return texture;
};

/**
 * Creates empty texture.
 * @public
 * @param {Number} [width=1] - Specifies the width of the texture image..
 * @param {Number} [height=1] - Specifies the width of the texture image..
 * @param {String} [filter="NEAREST"] - Specifies GL_TEXTURE_MIN(MAX)_FILTER texture value.
 * @param {String} [internalFormat="RGBA"] - Specifies the color components in the texture.
 * @param {String} [format="RGBA"] - Specifies the format of the texel data.
 * @param {String} [type="UNSIGNED_BYTE"] - Specifies the data type of the texel data.
 * @param {Number} [levels=0] - Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
 * @returns {Object} - WebGL texture object.
 */
Handler.prototype.createEmptyTexture2DExt = function (
    width = 1,
    height = 1,
    filter = "NEAREST",
    internalFormat = "RGBA",
    format = "RGBA",
    type = "UNSIGNED_BYTE",
    level = 0
) {
    var gl = this.gl;
    var texture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);

    gl.texImage2D(gl.TEXTURE_2D, level, gl[internalFormat.toUpperCase()], width, height, 0,
        gl[format.toUpperCase()], gl[type.toUpperCase()], null);

    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl[filter.toUpperCase()]);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl[filter.toUpperCase()]);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);

    gl.bindTexture(gl.TEXTURE_2D, null);
    return texture;
};

///**
// * Creates Empty half float texture.
// * @public
// * @param {number} width - Empty texture width.
// * @param {number} height - Empty texture height.
// * @returns {Object} - WebGL half float texture object.
// */
//Handler.prototype.createEmptyTexture_hf = function (width, height) {
//    var gl = this.gl;
//    var texture = gl.createTexture();
//    gl.bindTexture(gl.TEXTURE_2D, texture);
//    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
//    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, width, height, 0, gl.RGBA, gl.HALF_FLOAT_OES, null);
//    //gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
//    //gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
//    //gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
//    //gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
//    gl.bindTexture(gl.TEXTURE_2D, null);
//    return texture;
//}

///**
// * Creates Empty float texture.
// * @public
// * @param {number} width - Empty texture width.
// * @param {number} height - Empty texture height.
// * @returns {Object} - WebGL float texture object.
// */
//Handler.prototype.createEmptyTexture_f = function (width, height) {
//    var gl = this.gl;
//    var texture = gl.createTexture();
//    gl.bindTexture(gl.TEXTURE_2D, texture);
//    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
//    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, width, height, 0, gl.RGBA, gl.FLOAT, null);
//    //gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
//    //gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
//    //gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
//    //gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
//    gl.bindTexture(gl.TEXTURE_2D, null);
//    return texture;
//}

/**
 * Creates Empty NEAREST filtered texture.
 * @public
 * @param {number} width - Empty texture width.
 * @param {number} height - Empty texture height.
 * @returns {Object} - WebGL texture object.
 */
Handler.prototype.createEmptyTexture_n = function (width, height) {
    var gl = this.gl;
    var texture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, width, height, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    gl.bindTexture(gl.TEXTURE_2D, null);
    return texture;
};

/**
 * Creates empty LINEAR filtered texture.
 * @public
 * @param {number} width - Empty texture width.
 * @param {number} height - Empty texture height.
 * @returns {Object} - WebGL texture object.
 */
Handler.prototype.createEmptyTexture_l = function (width, height) {
    var gl = this.gl;
    var texture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, width, height, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    gl.bindTexture(gl.TEXTURE_2D, null);
    return texture;
};

/**
 * Creates LINEAR filter texture.
 * @public
 * @param {Object} image - Image or Canvas object.
 * @returns {Object} - WebGL texture object.
 */
Handler.prototype.createTexture_l = function (image) {
    var gl = this.gl;
    var texture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    gl.bindTexture(gl.TEXTURE_2D, null);
    return texture;
};

/**
 * Creates MIPMAP filter texture.
 * @public
 * @param {Object} image - Image or Canvas object.
 * @returns {Object} - WebGL texture object.
 */
Handler.prototype.createTexture_mm = function (image) {
    var gl = this.gl;
    var texture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
    gl.generateMipmap(gl.TEXTURE_2D);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR_MIPMAP_LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    gl.bindTexture(gl.TEXTURE_2D, null);
    return texture;
};

/**
 * Creates ANISOTROPY filter texture.
 * @public
 * @param {Object} image - Image or Canvas object.
 * @returns {Object} - WebGL texture object.
 */
Handler.prototype.createTexture_a = function (image) {
    var gl = this.gl;
    var texture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
    gl.generateMipmap(gl.TEXTURE_2D);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR_MIPMAP_LINEAR);
    gl.texParameterf(gl.TEXTURE_2D, this.extensions.EXT_texture_filter_anisotropic.TEXTURE_MAX_ANISOTROPY_EXT, this._params.anisotropy);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    gl.bindTexture(gl.TEXTURE_2D, null);
    return texture;
};

/**
 * Creates DEFAULT filter texture, ANISOTROPY is default.
 * @public
 * @param {Object} image - Image or Canvas object.
 * @returns {Object} - WebGL texture object.
 */
Handler.prototype.createTexture = function (image) {
    return this.createTexture_a(image);
};

/**
 * Creates cube texture.
 * @public
 * @param {Object.<string>} params - Face image urls:
 * @param {string} params.px - Positive X or right image url.
 * @param {string} params.nx - Negative X or left image url.
 * @param {string} params.py - Positive Y or up image url.
 * @param {string} params.ny - Negative Y or bottom image url.
 * @param {string} params.pz - Positive Z or face image url.
 * @param {string} params.nz - Negative Z or back image url.
 * @returns {Object} - WebGL texture object.
 */
Handler.prototype.loadCubeMapTexture = function (params) {
    var gl = this.gl;
    var texture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_CUBE_MAP, texture);
    gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_MAG_FILTER, gl.LINEAR);

    var faces = [[params.px, gl.TEXTURE_CUBE_MAP_POSITIVE_X],
    [params.nx, gl.TEXTURE_CUBE_MAP_NEGATIVE_X],
    [params.py, gl.TEXTURE_CUBE_MAP_POSITIVE_Y],
    [params.ny, gl.TEXTURE_CUBE_MAP_NEGATIVE_Y],
    [params.pz, gl.TEXTURE_CUBE_MAP_POSITIVE_Z],
    [params.nz, gl.TEXTURE_CUBE_MAP_NEGATIVE_Z]];

    var imageCanvas = new ImageCanvas();
    imageCanvas.fillEmpty();
    var emptyImage = imageCanvas.getImage();

    for (let i = 0; i < faces.length; i++) {
        let face = faces[i][1];
        gl.bindTexture(gl.TEXTURE_CUBE_MAP, texture);
        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
        gl.texImage2D(face, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, emptyImage);
    }

    for (let i = 0; i < faces.length; i++) {
        let face = faces[i][1];
        let image = new Image();
        image.crossOrigin = '';
        image.onload = (function (texture, face, image) {
            return function () {
                gl.bindTexture(gl.TEXTURE_CUBE_MAP, texture);
                gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
                gl.texImage2D(face, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
            };
        }(texture, face, image));
        image.src = faces[i][0];
    }
    return texture;
};

/**
 * Adds shader program to the handler.
 * @public
 * @param {og.webgl.Program} program - Shader program.
 * @param {boolean} [notActivate] - If it's true program will not compile.
 * @return {og.webgl.Program} -
 */
Handler.prototype.addProgram = function (program, notActivate) {
    if (!this.programs[program.name]) {
        var sc = new ProgramController(this, program);
        this.programs[program.name] = sc;
        this._initProgramController(sc);
        if (notActivate) {
            sc._activated = false;
        }
    } else {
        console.log("og.webgl.Handler:284 - shader program: '" + program.name + "' is allready exists.");
    }
    return program;
};

/**
 * Removes shader program from handler.
 * @public
 * @param {String} name - Shader program name.
 */
Handler.prototype.removeProgram = function (name) {
    this.programs[name] && this.programs[name].remove();
};

/**
 * Adds shader programs to the handler.
 * @public
 * @param {Array.<og.webgl.Program>} programsArr - Shader program array.
 */
Handler.prototype.addPrograms = function (programsArr) {
    for (var i = 0; i < programsArr.length; i++) {
        this.addProgram(programsArr[i]);
    }
};

/**
 * Used in addProgram
 * @private
 * @param {og.webgl.ProgramController} sc - Program controller
 */
Handler.prototype._initProgramController = function (sc) {
    if (this._initialized) {
        sc.initialize();
        if (!this.activeProgram) {
            this.activeProgram = sc;
            sc.activate();
        } else {
            sc.deactivate();
            this.activeProgram._program.enableAttribArrays();
            this.activeProgram._program.use();
        }
    }
};

/**
 * Used in init function.
 * @private
 */
Handler.prototype._initPrograms = function () {
    for (var p in this.programs) {
        this._initProgramController(this.programs[p]);
    }
};

/**
 * Initialize additional WebGL extensions.
 * @public
 * @param {string} extensionStr - Extension name.
 * @param {boolean} showLog - Show logging.
 * @return {Object} -
 */
Handler.prototype.initializeExtension = function (extensionStr, showLog) {
    if (!(this.extensions && this.extensions[extensionStr])) {
        var ext = Handler.getExtension(this.gl, extensionStr);
        if (ext) {
            this.extensions[extensionStr] = ext;
        } else if (showLog) {
            console.log("og.webgl.Handler: extension '" + extensionStr + "' doesn't initialize.");
        }
    }
    return this.extensions && this.extensions[extensionStr];
};

/**
 * Main function that initialize handler.
 * @public
 */
Handler.prototype.initialize = function () {

    if (this._id) {
        this.canvas = document.getElementById(this._id);
    } else {
        this.canvas = document.createElement("canvas");
        this.canvas.width = this._params.width;
        this.canvas.height = this._params.height;
    }

    this.gl = Handler.getContext(this.canvas, this._params.context);
    this._initialized = true;

    /** Sets deafult extensions */
    this._params.extensions.push("EXT_texture_filter_anisotropic");

    if (this.gl.type === "webgl") {
        this._params.extensions.push("OES_standard_derivatives");
        this._params.extensions.push("OES_element_index_uint");
    } else {
        this._params.extensions.push("EXT_color_buffer_float");
        this._params.extensions.push("OES_texture_float_linear");
    }

    var i = this._params.extensions.length;
    while (i--) {
        this.initializeExtension(this._params.extensions[i], true);
    }

    if (!this.extensions.EXT_texture_filter_anisotropic) {
        this.createTexture = this.createTexture_mm;
    }

    /** Initilalize shaders and rendering parameters*/
    this._initPrograms();
    this._setDefaults();
};

/**
 * Sets default gl render parameters. Used in init function.
 * @private
 */
Handler.prototype._setDefaults = function () {
    this.activateDepthTest();
    this.setSize(this.canvas.clientWidth || this._params.width, this.canvas.clientHeight || this._params.height);
    this.gl.frontFace(this.gl.CCW);
    this.gl.cullFace(this.gl.BACK);
    this.activateFaceCulling();
    this.deactivateBlending();
    var that = this;
    this.createDefaultTexture({ color: "rgba(0,0,0,0.0)" }, function (t) {
        that.transparentTexture = t;
    });
};

/**
 * Activate depth test.
 * @public
 */
Handler.prototype.activateDepthTest = function () {
    this.gl.enable(this.gl.DEPTH_TEST);
};

/**
 * Deactivate depth test.
 * @public
 */
Handler.prototype.deactivateDepthTest = function () {
    this.gl.disable(this.gl.DEPTH_TEST);
};

/**
 * Activate face culling.
 * @public
 */
Handler.prototype.activateFaceCulling = function () {
    this.gl.enable(this.gl.CULL_FACE);
};

/**
 * Deactivate face cullting.
 * @public
 */
Handler.prototype.deactivateFaceCulling = function () {
    this.gl.disable(this.gl.CULL_FACE);
};

/**
 * Activate blending.
 * @public
 */
Handler.prototype.activateBlending = function () {
    this.gl.enable(this.gl.BLEND);
};

/**
 * Deactivate blending.
 * @public
 */
Handler.prototype.deactivateBlending = function () {
    this.gl.disable(this.gl.BLEND);
};

/**
 * Creates STREAM_DRAW ARRAY buffer.
 * @public
 * @param {Array.<number>} array - Input array.
 * @param {number} itemSize - Array item size.
 * @param {number} numItems - Items quantity.
 * @param {number} [usage=STATIC_DRAW] - Parameter of the bufferData call can be one of STATIC_DRAW, DYNAMIC_DRAW, or STREAM_DRAW.
 * @return {Object} -
 */
Handler.prototype.createStreamArrayBuffer = function (itemSize, numItems, usage, bites = 4) {
    var buffer = this.gl.createBuffer();
    this.gl.bindBuffer(this.gl.ARRAY_BUFFER, buffer);
    this.gl.bufferData(this.gl.ARRAY_BUFFER, numItems * itemSize * bites, usage || this.gl.STREAM_DRAW);
    this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
    buffer.itemSize = itemSize;
    buffer.numItems = numItems;
    return buffer;
};

/**
 * Load stream ARRAY buffer.
 * @public
 * @param {Array.<number>} array - Input array.
 * @param {number} itemSize - Array item size.
 * @param {number} numItems - Items quantity.
 * @param {number} [usage=STATIC_DRAW] - Parameter of the bufferData call can be one of STATIC_DRAW, DYNAMIC_DRAW, or STREAM_DRAW.
 * @return {Object} -
 */
Handler.prototype.setStreamArrayBuffer = function (buffer, array, offset = 0) {
    let gl = this.gl;
    gl.bindBuffer(this.gl.ARRAY_BUFFER, buffer);
    gl.bufferSubData(this.gl.ARRAY_BUFFER, offset, array);
    gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
    return buffer;
};

/**
 * Creates ARRAY buffer.
 * @public
 * @param {Array.<number>} array - Input array.
 * @param {number} itemSize - Array item size.
 * @param {number} numItems - Items quantity.
 * @param {number} [usage=STATIC_DRAW] - Parameter of the bufferData call can be one of STATIC_DRAW, DYNAMIC_DRAW, or STREAM_DRAW.
 * @return {Object} -
 */
Handler.prototype.createArrayBuffer = function (array, itemSize, numItems, usage) {
    var buffer = this.gl.createBuffer();
    this.gl.bindBuffer(this.gl.ARRAY_BUFFER, buffer);
    this.gl.bufferData(this.gl.ARRAY_BUFFER, array, usage || this.gl.STATIC_DRAW);
    this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
    buffer.itemSize = itemSize;
    buffer.numItems = numItems;
    return buffer;
};

/**
 * Creates ELEMENT ARRAY buffer.
 * @public
 * @param {Array.<number>} array - Input array.
 * @param {number} itemSize - Array item size.
 * @param {number} numItems - Items quantity.
 * @param {number} [usage=STATIC_DRAW] - Parameter of the bufferData call can be one of STATIC_DRAW, DYNAMIC_DRAW, or STREAM_DRAW.
 * @return {Object} -
 */
Handler.prototype.createElementArrayBuffer = function (array, itemSize, numItems, usage) {
    var buffer = this.gl.createBuffer();
    this.gl.bindBuffer(this.gl.ELEMENT_ARRAY_BUFFER, buffer);
    this.gl.bufferData(this.gl.ELEMENT_ARRAY_BUFFER, array, usage || this.gl.STATIC_DRAW);
    this.gl.bindBuffer(this.gl.ELEMENT_ARRAY_BUFFER, null);
    buffer.itemSize = itemSize;
    buffer.numItems = numItems || array.length;
    return buffer;
};

/**
 * Sets handler canvas size.
 * @public
 * @param {number} w - Canvas width.
 * @param {number} h - Canvas height.
 */
Handler.prototype.setSize = function (w, h) {

    if (w > MAX_SIZE) {
        w = MAX_SIZE;
    }

    if (h > MAX_SIZE) {
        h = MAX_SIZE;
    }

    this._params.width = w;
    this._params.height = h;
    this.canvas.width = w;
    this.canvas.height = h;
    this._oneByHeight = 1 / h;

    this.gl && this.gl.viewport(0, 0, w, h);
    this.onCanvasResize && this.onCanvasResize(this.canvas);
};

/**
 * Returns context screen width.
 * @public
 * @returns {number} -
 */
Handler.prototype.getWidth = function () {
    return this.canvas.width;
};

/**
 * Returns context screen height.
 * @public
 * @returns {number} -
 */
Handler.prototype.getHeight = function () {
    return this.canvas.height;
};

/**
 * Returns canvas aspect ratio.
 * @public
 * @returns {number} -
 */
Handler.prototype.getClientAspect = function () {
    return this.canvas.clientWidth / this.canvas.clientHeight;
};

/**
 * Returns screen center coordinates.
 * @public
 * @returns {number} -
 */
Handler.prototype.getCenter = function () {
    var c = this.canvas;
    return new Vec2(Math.round(c.width * 0.5), Math.round(c.height * 0.5));
};

/**
 * Draw single frame.
 * @public
 * @param {number} now - Frame current time milliseconds.
 */
Handler.prototype.drawFrame = function () {

    /** Calculating frame time */
    var now = window.performance.now();
    this.deltaTime = now - this._lastAnimationFrameTime;
    this._lastAnimationFrameTime = now;

    this.defaultClock._tick(this.deltaTime);

    for (var i = 0; i < this._clocks.length; i++) {
        this._clocks[i]._tick(this.deltaTime);
    }

    /** Canvas resize checking */
    var canvas = this.canvas;
    if (canvas.clientWidth !== canvas.width ||
        canvas.clientHeight !== canvas.height) {
        this.setSize(canvas.clientWidth, canvas.clientHeight);
    }

    /** Draw frame */
    this._frameCallback();
};

/**
 * Clearing gl frame.
 * @public
 */
Handler.prototype.clearFrame = function () {
    var gl = this.gl;
    this.gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
};

/**
 * Starts animation loop.
 * @public
 */
Handler.prototype.start = function () {
    if (!this._requestAnimationFrameId && this._initialized) {
        this._lastAnimationFrameTime = window.performance.now();
        this.defaultClock.setDate(new Date());
        this._animationFrameCallback();
    }
};

Handler.prototype.stop = function () {
    if (this._requestAnimationFrameId) {
        window.cancelAnimationFrame(this._requestAnimationFrameId);
        this._requestAnimationFrameId = null;
    }
};

/**
 * Make animation.
 * @private
 */
Handler.prototype._animationFrameCallback = function () {
    this._requestAnimationFrameId = window.requestAnimationFrame(() => {
        this.drawFrame();
        this._animationFrameCallback();
    });
};

/**
 * Creates default texture object
 * @public
 * @param {Object} [params] - Texture parameters:
 * @param {Array.<number, number, number, number>} [params.color] - Texture RGBA color
 * @param {number} [params.url] - Texture source url
 * @param {callback} success - Creation callback
 */
Handler.prototype.createDefaultTexture = function (params, success) {
    var imgCnv;
    var texture;
    if (params && params.color) {
        imgCnv = new ImageCanvas(2, 2);
        imgCnv.fillColor(params.color);
        texture = this.createTexture_n(imgCnv._canvas);
        texture.default = true;
        success(texture);
    } else if (params && params.url) {
        var img = new Image();
        var that = this;
        img.onload = function () {
            texture = that.createTexture(this);
            texture.default = true;
            success(texture);
        };
        img.src = params.url;
    } else {
        imgCnv = new ImageCanvas(2, 2);
        imgCnv.fillColor("#C5C5C5");
        texture = this.createTexture_n(imgCnv._canvas);
        texture.default = true;
        success(texture);
    }
};

/**
 * @public
 */
Handler.prototype.destroy = function () {

    var gl = this.gl;

    this.stop();

    for (var p in this.programs) {
        this.removeProgram(p);
    }

    gl.deleteTexture(this.transparentTexture);
    this.transparentTexture = null;

    this.framebufferStack = null;
    this.framebufferStack = new Stack();

    if (this.canvas.parentNode) {
        this.canvas.parentNode.removeChild(this.canvas);
    }
    this.canvas.width = 1;
    this.canvas.height = 1;
    this.canvas = null;

    var numAttribs = gl.getParameter(gl.MAX_VERTEX_ATTRIBS);
    var tmp = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, tmp);
    for (let ii = 0; ii < numAttribs; ++ii) {
        gl.disableVertexAttribArray(ii);
        gl.vertexAttribPointer(ii, 4, gl.FLOAT, false, 0, 0);
        gl.vertexAttrib1f(ii, 0);
    }
    gl.deleteBuffer(tmp);

    var numTextureUnits = gl.getParameter(gl.MAX_TEXTlURE_IMAGE_UNITS);
    for (let ii = 0; ii < numTextureUnits; ++ii) {
        gl.activeTexture(gl.TEXTURE0 + ii);
        gl.bindTexture(gl.TEXTURE_CUBE_MAP, null);
        gl.bindTexture(gl.TEXTURE_2D, null);
    }

    gl.activeTexture(gl.TEXTURE0);
    gl.useProgram(null);
    gl.bindBuffer(gl.ARRAY_BUFFER, null);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, null);
    gl.bindFramebuffer(gl.FRAMEBUFFER, null);
    gl.bindRenderbuffer(gl.RENDERBUFFER, null);
    gl.disable(gl.BLEND);
    gl.disable(gl.CULL_FACE);
    gl.disable(gl.DEPTH_TEST);
    gl.disable(gl.DITHER);
    gl.disable(gl.SCISSOR_TEST);
    gl.blendColor(0, 0, 0, 0);
    gl.blendEquation(gl.FUNC_ADD);
    gl.blendFunc(gl.ONE, gl.ZERO);
    gl.clearColor(0, 0, 0, 0);
    gl.clearDepth(1);
    gl.clearStencil(-1);

    this.gl = null;

    this._initialized = false;
};

Handler.prototype.addClock = function (clock) {
    if (!clock.__handler) {
        clock.__handler = this;
        this._clocks.push(clock);
    }
};

Handler.prototype.addClocks = function (clockArr) {
    for (var i = 0; i < clockArr.length; i++) {
        this.addClock(clockArr[i]);
    }
};

Handler.prototype.removeClock = function (clock) {
    if (clock.__handler) {
        var c = this._clocks;
        var i = c.length;
        while (i--) {
            if (c[i].equal(clock)) {
                clock.__handler = null;
                c.splice(i, 1);
                break;
            }
        }
    }
};

export { Handler };