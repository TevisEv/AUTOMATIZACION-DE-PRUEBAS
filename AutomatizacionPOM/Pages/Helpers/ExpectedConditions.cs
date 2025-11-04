using OpenQA.Selenium;
using System;

namespace AutomatizacionPOM.Pages.Helpers
{
    // Minimal replacement for Selenium's ExpectedConditions helpers
    public static class ExpectedConditions
    {
        public static Func<IWebDriver, IWebElement> ElementIsVisible(By locator)
        {
            return driver =>
            {
                try
                {
                    var element = driver.FindElement(locator);
                    return element.Displayed ? element : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            };
        }

        public static Func<IWebDriver, IWebElement> ElementToBeClickable(By locator)
        {
            return driver =>
            {
                try
                {
                    var element = driver.FindElement(locator);
                    if (element != null && element.Displayed && element.Enabled)
                        return element;
                    return null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            };
        }
    }
}
