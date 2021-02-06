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

        public JavascriptIntegrationService(ChromiumWebBrowser browser)
        {
            eventHandler = new JavascriptEventHandler();
            eventHandler.EventArrived += JavascriptEventHandlerEventArrived;
            this.browser = browser;
            browser.JavascriptObjectRepository.Register("boundEventHandler", eventHandler, true, BindingOptions.DefaultBinder);
        }

        public void CallProcedure(string expression)
        {
            browser.ExecuteScriptAsync(expression);
        }

        public JavascriptResponse CallFunction(string expression)
        {
            return GetDataAsync(expression).Result;
        }

        private void JavascriptEventHandlerEventArrived(string eventName, dynamic eventArgs)
        {
            if (eventName == "MarkerClick")
            {
                OnMarkerClicked(eventArgs);
            }
        }

        async Task<JavascriptResponse> GetDataAsync(string expression)
        {
            return await browser.EvaluateScriptAsync(expression);
        }

        protected virtual void OnMarkerClicked(string location)
        {
            MarkerClicked?.Invoke(this, location);
        }
    }
}
