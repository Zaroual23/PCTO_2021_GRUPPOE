using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobeViewer.Interfaces;
using CefSharp;
using CefSharp.WinForms;
using System.Windows.Forms;

namespace GlobeViewer.Classes
{
    class JavascriptIntegrationService : IJavascriptIntegrationService
    {
        public event MarkerClickedEventHandler MarkerClicked;
        private readonly JavascriptEventHandler eventHandler = default(JavascriptEventHandler);
        private readonly ChromiumWebBrowser browser = default(ChromiumWebBrowser);
        private TaskCompletionSource<bool> canExecuteJavascript = new TaskCompletionSource<bool>();

        /// <summary>
        /// Initializes the Javascript integration service
        /// </summary>
        /// <param name="browser">Browser that integrates the Javascript integration service</param>
        public JavascriptIntegrationService(ChromiumWebBrowser browser)
        {
            this.browser = browser ?? throw new ArgumentNullException("'browser' parameter can't be null"); ;

            //Setting up Javascript event handling
            eventHandler = new JavascriptEventHandler();
            eventHandler.EventArrived += JavascriptEventHandlerEventArrived;
            browser.JavascriptObjectRepository.Register("boundEventHandler", eventHandler, true, BindingOptions.DefaultBinder);
            browser.FrameLoadEnd += (sender, args) =>
            {
                //Wait for the MainFrame to finish loading
                if (args.Frame.IsMain)
                {
                    canExecuteJavascript.SetResult(true);
                }
            };
        }

        /// <summary>
        /// Call a procedure (no return value)
        /// </summary>
        /// <param name="expression"></param>
        public void CallProcedure(string expression)
        {
            if (expression == null)
                throw new ArgumentException("'expression' parameter can't be null");

            var _ = SendDataAsync(expression).Status;
        }

        /// <summary>
        /// Call a function (returns a "JavascriptResponse")
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public JavascriptResponse CallFunction(string expression)
        {
            if (expression == null)
                throw new ArgumentException("'expression' parameter can't be null");

            return GetDataAsync(expression).Result;
        }

        //Identifies the event and calls its respective handler
        private void JavascriptEventHandlerEventArrived(string eventName, dynamic eventArgs)
        {
            if (eventName == "MarkerClick")
            {
                OnMarkerClicked(eventArgs);
            }
        }

        //Calls Javascript function and waits to get a return value, asynchronously
        async Task<JavascriptResponse> GetDataAsync(string expression)
        {
            await canExecuteJavascript.Task;
            return await browser.EvaluateScriptAsync(expression);
        }

        async Task SendDataAsync(string expression)
        {
            await canExecuteJavascript.Task;
            browser.ExecuteScriptAsync(expression);
        }

        //Invokes the public "MarkerClicked"
        protected virtual void OnMarkerClicked(string location)
        {
            MarkerClicked?.Invoke(this, location);
        }
    }
}
