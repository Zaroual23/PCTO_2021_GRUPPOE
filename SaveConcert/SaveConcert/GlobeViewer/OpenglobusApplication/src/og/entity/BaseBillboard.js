/**
 * @module og/entity/BaseBillboard
 */

'use strict';

import * as utils from '../utils/shared.js';
import { Vec3 } from '../math/Vec3.js';

/**
 * Base prototype for billboard and label classes.
 * @class
 * @param {Object} [options] - Options:
 * @param {og.Vec3|Array.<number>} [options.position] - Billboard spatial position.
 * @param {number} [options.rotation] - Screen angle rotaion.
 * @param {og.Vec4|string|Array.<number>} [options.color] - Billboard color.
 * @param {og.Vec3|Array.<number>} [options.alignedAxis] - Billboard aligned vector.
 * @param {og.Vec3|Array.<number>} [options.offset] - Billboard center screen offset.
 * @param {boolean} [options.visibility] - Visibility.
 */
class BaseBillboard {
    constructor(options) {
        options = options || {};

        /**
         * Object unic identifier.
         * @public
         * @readonly
         * @type {number}
         */
        this.id = BaseBillboard._staticCounter++;

        /**
         * Billboard center cartesian position.
         * @protected
         * @type {og.Vec3}
         */
        this._position = utils.createVector3(options.position);

        this._positionHigh = new Vec3();
        
        this._positionLow = new Vec3();
        
        Vec3.doubleToTwoFloats(this._position, this._positionHigh, this._positionLow);

        /**
         * Screen space rotation angle.
         * @protected
         * @type {number}
         */
        this._rotation = options.rotation || 0;

        /**
         * RGBA color.
         * @protected
         * @type {og.Vec4}
         */
        this._color = utils.createColorRGBA(options.color);

        /**
         * Cartesian aligned axis vector.
         * @protected
         * @type {og.Vec3}
         */
        this._alignedAxis = utils.createVector3(options.alignedAxis);

        /**
         * Billboard center screen space offset. Where x,y - screen space offset and z - depth offset.
         * @protected
         * @type {og.math.Vecto3}
         */
        this._offset = utils.createVector3(options.offset);

        /**
         * Billboard visibility.
         * @protected
         * @type {boolean}
         */
        this._visibility = options.visibility != undefined ? options.visibility : true;

        /**
         * Entity instance that holds this billboard.
         * @protected
         * @type {og.Entity}
         */
        this._entity = null;

        /**
         * Handler that stores and renders this billboard object.
         * @protected
         * @type {og.BillboardHandler}
         */
        this._handler = null;

        /**
         * Billboard handler array index.
         * @protected
         * @type {number}
         */
        this._handlerIndex = -1;
    }

    static get _staticCounter() {
        if (!this._counter && this._counter !== 0) {
            this._counter = 0;
        }
        return this._counter;
    }

    static set _staticCounter(n) {
        this._counter = n;
    }

    /**
     * Sets billboard position.
     * @public
     * @param {number} x - X coordinate.
     * @param {number} y - Y coordinate.
     * @param {number} z - Z coordinate.
     */
    setPosition(x, y, z) {
        this._position.x = x;
        this._position.y = y;
        this._position.z = z;        
        Vec3.doubleToTwoFloats(position, this._positionHigh, this._positionLow);
        this._handler && this._handler.setPositionArr(this._handlerIndex, this._positionHigh, this._positionLow);
    }

    /**
     * Sets billboard position.
     * @public
     * @param {og.Vec3} position - Cartesian coordinates.
     */
    setPosition3v(position) {
        this._position.x = position.x;
        this._position.y = position.y;
        this._position.z = position.z;
        Vec3.doubleToTwoFloats(position, this._positionHigh, this._positionLow);
        this._handler && this._handler.setPositionArr(this._handlerIndex, this._positionHigh, this._positionLow);
    }

    /**
     * Returns billboard position.
     * @public
     * @returns {og.Vec3}
     */
    getPosition() {
        return this._position;
    }

    /**
     * Sets screen space offset.
     * @public
     * @param {number} x - X offset.
     * @param {number} y - Y offset.
     * @param {number} [z] - Z offset.
     */
    setOffset(x, y, z) {
        this._offset.x = x;
        this._offset.y = y;
        (z != undefined) && (this._offset.z = z);
        this._handler && this._handler.setOffsetArr(this._handlerIndex, this._offset);
    }

    /**
     * Sets screen space offset.
     * @public
     * @param {og.Vec2} offset - Offset size.
     */
    setOffset3v(offset) {
        this._offset.x = offset.x;
        this._offset.y = offset.y;
        (offset.z != undefined) && (this._offset.z = offset.z);
        this._handler && this._handler.setOffsetArr(this._handlerIndex, offset);
    }

    /**
     * Returns billboard screen space offset size.
     * @public
     * @returns {og.Vec3}
     */
    getOffset() {
        return this._offset;
    }

    /**
     * Sets billboard screen space rotation in radians.
     * @public
     * @param {number} rotation - Screen space rotation in radians.
     */
    setRotation(rotation) {
        this._rotation = rotation;
        this._handler && this._handler.setRotationArr(this._handlerIndex, rotation);
    }

    /**
     * Gets screen space rotation.
     * @public
     * @returns {number}
     */
    getRotation() {
        return this._rotation;
    }

    /**
     * Sets billboard opacity.
     * @public
     * @param {number} a - Billboard opacity.
     */
    setOpacity(a) {
        this._color.w = a;
        this.setColor(this._color.x, this._color.y, this._color.z, a);
    }

    /**
     * Sets RGBA color. Each channel from 0.0 to 1.0.
     * @public
     * @param {number} r - Red.
     * @param {number} g - Green.
     * @param {number} b - Blue.
     * @param {number} a - Alpha.
     */
    setColor(r, g, b, a) {
        this._color.x = r;
        this._color.y = g;
        this._color.z = b;
        (a != undefined) && (this._color.w = a);
        this._handler && this._handler.setRgbaArr(this._handlerIndex, this._color);
    }

    /**
     * Sets RGBA color. Each channel from 0.0 to 1.0.
     * @public
     * @param {og.Vec4} color - RGBA vector.
     */
    setColor4v(color) {
        this._color.x = color.x;
        this._color.y = color.y;
        this._color.z = color.z;
        (color.w != undefined) && (this._color.w = color.w);
        this._handler && this._handler.setRgbaArr(this._handlerIndex, color);
    }

    /**
     * Sets billboard color.
     * @public
     * @param {string} color - HTML style color.
     */
    setColorHTML(color) {
        this.setColor4v(utils.htmlColorToRgba(color));
    }

    /**
     * Returns RGBA color.
     * @public
     * @returns {og.Vec4}
     */
    getColor() {
        return this._color;
    }

    /**
     * Sets billboard visibility.
     * @public
     * @param {boolean} visibility - Visibility flag.
     */
    setVisibility(visibility) {
        this._visibility = visibility;
        this._handler && this._handler.setVisibility(this._handlerIndex, visibility);
    }

    /**
     * Returns billboard visibility.
     * @public
     * @returns {boolean}
     */
    getVisibility() {
        return this._visibility;
    }

    /**
     * Sets billboard cartesian aligned vector.
     * @public
     * @param {number} x - Aligned vector X coordinate.
     * @param {number} y - Aligned vector Y coordinate.
     * @param {number} z - Aligned vector Z coordinate.
     */
    setAlignedAxis(x, y, z) {
        this._alignedAxis.x = x;
        this._alignedAxis.y = y;
        this._alignedAxis.z = z;
        this._handler && this._handler.setAlignedAxisArr(this._handlerIndex, this._alignedAxis);
    }

    /**
     * Sets billboard aligned vector.
     * @public
     * @param {og.math.Vecto3} alignedAxis - Vector to align.
     */
    setAlignedAxis3v(alignedAxis) {
        this._alignedAxis.x = alignedAxis.x;
        this._alignedAxis.y = alignedAxis.y;
        this._alignedAxis.z = alignedAxis.z;
        this._handler && this._handler.setAlignedAxisArr(this._handlerIndex, alignedAxis);
    }

    /**
     * Returns aligned vector.
     * @public
     * @returns {og.Vec3}
     */
    getAlignedAxis() {
        return this._alignedAxis;
    }

    /**
     * Removes billboard from hander.
     * @public
     */
    remove() {
        this._entity = null;
        this._handler && this._handler.remove(this);
    }

    /**
     * Sets billboard picking color.
     * @public
     * @param {og.Vec3} color - Picking color.
     */
    setPickingColor3v(color) {
        this._handler && this._handler.setPickingColorArr(this._handlerIndex, color);
    }
};

export { BaseBillboard };