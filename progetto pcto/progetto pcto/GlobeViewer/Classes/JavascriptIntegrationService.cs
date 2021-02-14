using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobeViewer.Interfaces;
using CefSharp;
using CefSharp.WinForms;

namespace GlobeViewer.Classes
{
    class JavascriptIntegrationService : IJavascriptIntegrationService
    {
        public event MarkerClickedEventHandler MarkerClicked;
        private readonly JavascriptEventHandler eventHandler = default(JavascriptEventHandler);
        private readonly ChromiumWebBrowser browser = default(ChromiumWebBrowser);

        /// <summary>
        /// Initializes the Javascript integration service
        /// </summary>
        /// <param name="browser">Browser that integrates the Javascript integration service</param>
        public JavascriptIntegrationService(ChromiumWebBrowser browser)
        {
            //Setting up Javascript event handling
            eventHandler = new JavascriptEventHandler();
            eventHandler.EventArrived += JavascriptEventHandlerEventArrived;
            this.browser = browser;
            browser.JavascriptObjectRepository.Register("boundEventHandler", eventHandler, true, BindingOptions.DefaultBinder);
        }

        /// <summary>
        /// Call a procedure (no return value)
        /// </summary>
        /// <param name="expression"></param>
        public void CallProcedure(string expression)
        {
            browser.ExecuteScriptAsync(expression);
        }

        /// <summary>
        /// Call a function (returns a "JavascriptResponse")
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public JavascriptResponse CallFunction(string expression)
        {
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
            return await browser.EvaluateScriptAsync(expression);
        }

        //Invokes the public "MarkerClicked"
        protected virtual void OnMarkerClicked(string location)
        {
            MarkerClicked?.Invoke(this, location);
        }
    }
}
