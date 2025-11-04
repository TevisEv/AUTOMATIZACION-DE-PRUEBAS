using AutomatizacionPOM.Pages.Helpers;
using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Threading;

namespace AutomatizacionPOM.Pages
{
    public class AccessPage
    {
        private IWebDriver driver;
        Utilities utilities;

        public AccessPage(IWebDriver driver)
        {
            this.driver = driver;
            utilities = new Utilities(driver);
        }

        // LOGIN
        private By usernameField = By.XPath("//input[@id='Email']");
        private By passwordField = By.XPath("//input[@id='Password']");
        private By loginButton = By.XPath("//button[normalize-space()='Iniciar']");
        private By acceptButton = By.XPath("//button[contains(text(),'Aceptar')]");
        private By logo = By.XPath("//img[@id='ImagenLogo']");

        // MÓDULOS
        private By CotizacionField = By.XPath("//a[@href='/Cotizacion/Index']");
        private By GastosField = By.XPath("//span[normalize-space()='Gasto']");

        // SUBMÓDULOS / SUBMENÚS
        private By NuevaCotizacionField = By.XPath("//button[normalize-space()='NUEVA COTIZACIÓN']");
        private By VerGastoField = By.XPath("//a[@href='/Gasto/Index']");
        private By VerGastoFieldAbs1 = By.XPath("/html/body/div[1]/aside/div/section/ul/li[9]/ul/li[1]/a");
        private By VerGastoFieldAbs2 = By.XPath("//*[@id='wrapper']/aside/div/section/ul/li[9]/ul/li[1]/a");
        private By NuevoGastoField = By.XPath("//span[normalize-space()='NUEVO GASTO']");
        private By BotonNuevoGasto = By.XPath("//button[contains(.,'NUEVO GASTO') or contains(.,'Nuevo Gasto') or contains(.,'NUEVO')]");

        public void OpenToAplicattion(string url)
        {
            driver.Navigate().GoToUrl(url);
            driver.Manage().Window.Maximize();
            Thread.Sleep(4000);
        }

        public void LoginToApplication(string _username, string _password)
        {
            utilities.EnterText(usernameField, _username);
            Thread.Sleep(1000);

            utilities.EnterText(passwordField, _password);
            Thread.Sleep(1000);

            utilities.ClickButton(loginButton);
            Thread.Sleep(3000);

            try
            {
                utilities.ClickButton(acceptButton);
                Thread.Sleep(2000);
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("Botón 'Aceptar' no encontrado, continuando...");
            }

            var successElement = driver.FindElement(logo);
            Assert.IsNotNull(successElement, "No se encontró el elemento de éxito después del login.");
        }

        public void enterModulo(string _modulo)
        {
            switch (_modulo.Trim().ToLower())
            {
                case "cotizacion":
                    utilities.ClickButton(CotizacionField);
                    break;

                case "gasto":
                case "gastos":
                    utilities.ClickButton(GastosField);
                    break;

                default:
                    throw new ArgumentException($"El módulo '{_modulo}' no es válido.");
            }

            Thread.Sleep(2000);
        }

        // Submódulos principales (botones dentro de los módulos)
        public void enterSubModulo(string _submodulo)
        {
            switch (_submodulo.Trim().ToLower())
            {
                case "nueva cotizacion":
                    utilities.ClickButton(NuevaCotizacionField);
                    break;

                case "ver gasto":
                    ClickVerGasto();
                    break;

                case "nuevo gasto":
                    ClickVerGasto(); // primero abre "Ver"
                    Thread.Sleep(1500);
                    ClickNuevoGasto();
                    break;

                default:
                    throw new ArgumentException($"El submódulo '{_submodulo}' no es válido.");
            }

            Thread.Sleep(3000);
        }

        // ==== NUEVO: Permite también acceder al submenú lateral ====
        public void enterSubmenuGasto(string _submenu)
        {
            string menu = _submenu.Trim().ToLower();
            if (menu == "ver")
            {
                ClickVerGasto();
            }
            else
            {
                throw new ArgumentException($"El submenú '{_submenu}' no es válido dentro de Gasto.");
            }
        }

        // ==== MÉTODOS AUXILIARES ====
        private void ClickVerGasto()
        {
            Console.WriteLine("Intentando acceder al submenú 'Ver' del módulo Gasto...");

            try
            {
                // Intento principal: href directo
                utilities.ClickButton(VerGastoField);
                Console.WriteLine("Click en //a[@href='/Gasto/Index']");
            }
            catch (Exception)
            {
                try
                {
                    // Fallback 1
                    utilities.ClickButton(VerGastoFieldAbs1);
                    Console.WriteLine("Click en /html/body/.../li[9]/ul/li[1]/a");
                }
                catch (Exception)
                {
                    try
                    {
                        // Fallback 2
                        utilities.ClickButton(VerGastoFieldAbs2);
                        Console.WriteLine("Click en //*[@id='wrapper']/aside/.../li[1]/a");
                    }
                    catch
                    {
                        throw new NoSuchElementException("No se encontró el enlace 'Ver' dentro del módulo Gasto.");
                    }
                }
            }

            Thread.Sleep(3000);
        }

        private void ClickNuevoGasto()
        {
            Console.WriteLine("Intentando hacer click en 'NUEVO GASTO'...");

            try
            {
                utilities.ClickButton(NuevoGastoField);
            }
            catch
            {
                try
                {
                    utilities.ClickButton(BotonNuevoGasto);
                }
                catch
                {
                    throw new NoSuchElementException("No se encontró el botón 'NUEVO GASTO' en la página.");
                }
            }

            Thread.Sleep(3000);
        }

        // Accesos directos
        public void enterNuevoGasto()
        {
            ClickVerGasto();
            Thread.Sleep(1500);
            ClickNuevoGasto();
        }

        public void enterNuevaCotizacion()
        {
            utilities.ClickButton(NuevaCotizacionField);
            Thread.Sleep(2000);
        }
    }
}
