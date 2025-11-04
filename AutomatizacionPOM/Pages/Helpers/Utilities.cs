using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;

namespace AutomatizacionPOM.Pages.Helpers
{
    public class Utilities
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private WebDriverWait shortWait;

        public Utilities(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            this.shortWait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public void ClickButton(By locator)
        {
            try
            {
                Console.WriteLine($"Intentando hacer click en: {locator}");

                // Esperar a que el elemento sea visible primero
                var element = WaitForElementVisible(locator);

                // Scroll al elemento
                ScrollViewElement(element);
                Thread.Sleep(500);

                // Intentar click normal
                element.Click();
                Console.WriteLine("Click exitoso");
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine($"Timeout con click normal: {ex.Message}");
                ClickWithJavaScript(locator);
            }
            catch (ElementNotInteractableException)
            {
                Console.WriteLine("Elemento no interactuable, usando JavaScript...");
                ClickWithJavaScript(locator);
            }
            catch (StaleElementReferenceException)
            {
                Console.WriteLine("Elemento obsoleto, reintentando...");
                Thread.Sleep(1000);
                ClickButton(locator); // Reintentar
            }
            Thread.Sleep(1500);
        }

        private void ClickWithJavaScript(By locator)
        {
            try
            {
                var element = driver.FindElement(locator);
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
                Console.WriteLine("Click con JavaScript exitoso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error con click JavaScript: {ex.Message}");
                throw new InvalidOperationException($"No se pudo hacer click en el elemento: {locator}", ex);
            }
        }

        public void EnterText(By locator, string text)
        {
            try
            {
                Console.WriteLine($"Ingresando texto: '{text}' en: {locator}");
                var element = WaitForElementVisible(locator);
                element.Clear();
                Thread.Sleep(500);
                element.SendKeys(text);
                Console.WriteLine("Texto ingresado exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ingresando texto: {ex.Message}");
                throw;
            }
            Thread.Sleep(1500);
        }

        public void ClearAndEnterText(By locator, string text)
        {
            try
            {
                Console.WriteLine($"Limpiando e ingresando texto: '{text}' en: {locator}");
                var element = WaitForElementVisible(locator);

                // Método más robusto para limpiar
                element.SendKeys(Keys.Control + "a");
                Thread.Sleep(500);
                element.SendKeys(Keys.Delete);
                Thread.Sleep(500);

                element.SendKeys(text);
                Console.WriteLine("Texto limpiado e ingresado exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error limpiando e ingresando texto: {ex.Message}");
                throw;
            }
            Thread.Sleep(1500);
        }

        public void Enter(By locator)
        {
            try
            {
                Console.WriteLine($"Presionando Enter en: {locator}");
                var element = WaitForElementVisible(locator);
                element.SendKeys(Keys.Enter);
                Console.WriteLine("Enter presionado exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error presionando Enter: {ex.Message}");
                throw;
            }
            Thread.Sleep(1500);
        }

        public void SelectOption(By dropdownLocator, string option)
        {
            try
            {
                Console.WriteLine($"Seleccionando opción: '{option}' del dropdown: {dropdownLocator}");

                // Esperar y hacer click en el dropdown
                var dropdown = WaitForElementVisible(dropdownLocator);
                dropdown.Click();
                Thread.Sleep(2000);

                // Buscar la opción con múltiples estrategias
                var optionSelectors = new[]
                {
                    By.XPath($"//li[contains(normalize-space(), '{option}')]"),
                    By.XPath($"//div[contains(normalize-space(), '{option}')]"),
                    By.XPath($"//span[contains(normalize-space(), '{option}')]"),
                    By.XPath($"//option[contains(normalize-space(), '{option}')]"),
                    By.XPath($"//*[contains(normalize-space(), '{option}') and not(self::input)]")
                };

                IWebElement optionElement = null;
                foreach (var selector in optionSelectors)
                {
                    try
                    {
                        // Usar wait corto para cada selector
                        optionElement = shortWait.Until(drv =>
                        {
                            var elements = drv.FindElements(selector);
                            return elements.FirstOrDefault(e => e.Displayed);
                        });

                        if (optionElement != null) break;
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (optionElement != null)
                {
                    ScrollViewElement(optionElement);
                    optionElement.Click();
                    Console.WriteLine($"Opcion '{option}' seleccionada exitosamente");
                }
                else
                {
                    // Fallback: escribir texto y presionar Enter
                    Console.WriteLine("Opcion no encontrada, intentando con entrada directa...");
                    dropdown.SendKeys(option);
                    Thread.Sleep(2000);
                    dropdown.SendKeys(Keys.Enter);
                    Console.WriteLine($"Opcion '{option}' ingresada directamente");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seleccionando opcion '{option}': {ex.Message}");
                throw;
            }
            Thread.Sleep(2000);
        }

        // SCROLL METHODS
        public void ScrollViewElement(IWebElement element)
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript(
                    "arguments[0].scrollIntoView({behavior: 'smooth', block: 'center', inline: 'center'});",
                    element
                );
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en scroll: {ex.Message}");
            }
        }

        public void ScrollToTop()
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0);");
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scroll to top: {ex.Message}");
            }
        }

        public void ScrollToBottom()
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scroll to bottom: {ex.Message}");
            }
        }

        // WAIT METHODS
        public IWebElement WaitForElementVisible(By locator)
        {
            try
            {
                Console.WriteLine($"Esperando elemento visible: {locator}");
                var element = wait.Until(ExpectedConditions.ElementIsVisible(locator));
                Console.WriteLine($"Elemento visible encontrado: {locator}");
                return element;
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine($"Timeout esperando elemento visible {locator}: {ex.Message}");
                throw new NoSuchElementException($"Elemento no visible después de 30 segundos: {locator}", ex);
            }
        }

        public IWebElement WaitForElementClickable(By locator)
        {
            try
            {
                Console.WriteLine($"Esperando elemento clickeable: {locator}");
                var element = wait.Until(ExpectedConditions.ElementToBeClickable(locator));
                Console.WriteLine($"Elemento clickeable encontrado: {locator}");
                return element;
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine($"Timeout esperando elemento clickeable {locator}: {ex.Message}");
                throw new ElementNotInteractableException($"Elemento no clickeable después de 30 segundos: {locator}", ex);
            }
        }

        public void WaitForLoadingToDisappear(By loadingLocator, int timeoutSeconds = 30)
        {
            try
            {
                Console.WriteLine($"Esperando a que desaparezca el loading...");
                var loadingWait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
                loadingWait.Until(drv =>
                {
                    try
                    {
                        return !drv.FindElement(loadingLocator).Displayed;
                    }
                    catch (NoSuchElementException)
                    {
                        return true;
                    }
                });
                Console.WriteLine("Loading desaparecio");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"Timeout esperando a que desaparezca el loading después de {timeoutSeconds} segundos");
            }
        }

        public void WaitForPageToLoad(int timeoutSeconds = 30)
        {
            try
            {
                Console.WriteLine("Esperando a que la página cargue completamente...");
                var pageWait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
                pageWait.Until(drv =>
                {
                    var jsExecutor = (IJavaScriptExecutor)drv;
                    return jsExecutor.ExecuteScript("return document.readyState").Equals("complete");
                });
                Console.WriteLine("Pagina cargada completamente");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"Timeout esperando carga de página después de {timeoutSeconds} segundos");
            }
        }

        // UTILITY METHODS
        public bool IsElementPresent(By locator)
        {
            try
            {
                driver.FindElement(locator);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public bool IsElementVisible(By locator)
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Displayed;
            }
            catch
            {
                return false;
            }
        }

        public string GetElementText(By locator)
        {
            try
            {
                var element = WaitForElementVisible(locator);
                return element.Text.Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        public void TakeScreenshot(string fileName)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile($"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                Console.WriteLine($"Screenshot guardado: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error tomando screenshot: {ex.Message}");
            }
        }

        // Método adicional para verificar si un elemento está habilitado
        public bool IsElementEnabled(By locator)
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Enabled;
            }
            catch
            {
                return false;
            }
        }

        // Método para obtener atributo de elemento de forma segura
        public string GetElementAttribute(By locator, string attributeName)
        {
            try
            {
                var element = WaitForElementVisible(locator);
                return element.GetAttribute(attributeName) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // Método para esperar un tiempo específico
        public void Wait(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        // Método para cambiar a iframe
        public void SwitchToFrame(By frameLocator)
        {
            try
            {
                var frame = WaitForElementVisible(frameLocator);
                driver.SwitchTo().Frame(frame);
                Console.WriteLine("Cambiado a iframe exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cambiando a iframe: {ex.Message}");
                throw;
            }
        }

        // Método para regresar al contenido principal
        public void SwitchToDefaultContent()
        {
            try
            {
                driver.SwitchTo().DefaultContent();
                Console.WriteLine("Regresado al contenido principal");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error regresando al contenido principal: {ex.Message}");
                throw;
            }
        }

        internal void ScrollViewTop()
        {
            throw new NotImplementedException();
        }
    }
}