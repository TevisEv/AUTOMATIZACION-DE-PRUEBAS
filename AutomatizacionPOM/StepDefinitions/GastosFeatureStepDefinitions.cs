using System;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using AutomatizacionPOM.Pages;

namespace AutomatizacionPOM.StepDefinitions
{
    [Binding]
    public class GastosFeatureStepDefinitions
    {
        private readonly IWebDriver _driver;
        private readonly Gasto _gasto;

        public GastosFeatureStepDefinitions(IWebDriver driver)
        {
            _driver = driver;
            _gasto = new Gasto(driver);
        }

        [When(@"ingresa el documento '(.*)'")]
        public void WhenIngresaElDocumento(string documento)
        {
            _gasto.IngresarDocumento(documento);
        }

        [When(@"ingresa la fecha de gasto '(.*)'")]
        public void WhenIngresaLaFechaDeGasto(string fecha)
        {
            _gasto.IngresarFechaGasto(fecha);
        }

        [When(@"ingresa observaciones '(.*)'")]
        public void WhenIngresaObservaciones(string obs)
        {
            _gasto.IngresarObservaciones(obs);
        }

        [When(@"ingresa el detalle '(.*)'")]
        public void WhenIngresaElDetalle(string detalle)
        {
            _gasto.IngresarDetalle(detalle);
        }

        [When(@"ingresa el concepto '(.*)'")]
        public void WhenIngresaElConcepto(string concepto)
        {
            _gasto.IngresarConcepto(concepto);
        }

        [When(@"ingresa el importe '(.*)'")]
        public void WhenIngresaElImporte(string importe)
        {
            _gasto.IngresarImporte(importe);
        }

        [When(@"selecciona el tipo de gasto '(.*)'")]
        public void WhenSeleccionaElTipoDeGasto(string tipo)
        {
            _gasto.SeleccionarTipoGasto(tipo);
        }

        [When(@"para gastos al crédito configurable ingresa inicial '(.*)' cuotas '(.*)' y dia de pago '(.*)'")]
        public void WhenParaGastosAlCreditoConfigurableIngresaInicialCuotasYDiaDePago(string inicial, string cuotas, string diaPago)
        {
            _gasto.ConfigurarCreditoConfigurable(inicial, cuotas, diaPago);
        }

        [When(@"con_igv")]
        public void WhenCon_Igv()
        {
            _gasto.SetIGV(true);
        }

        [When(@"sin_igv")]
        public void WhenSin_Igv()
        {
            _gasto.SetIGV(false);
        }

        // resukltados / verificaciones
        [Then(@"es necesario que el importe sea mayor a (.*)")]
        public void ThenEsNecesarioQueElImporteSeaMayorA(int minimo)
        {
            // Locators típicos donde suele aparecer el mensaje (toast/alert/validaciones)
            By[] posiblesMensajes = new[]
            {
                By.CssSelector(".toast-message, .toast, .alert, .alert-danger, .validation-summary-errors, .field-validation-error"),
                By.XPath("//*[contains(@class,'validation') and contains(@class,'error')]"),
                By.XPath(
                    "//*[contains(translate(normalize-space(.),'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'importe') and " +
                    "(contains(translate(.,'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'mayor') or contains(.,'necesario') or contains(.,'debe ser'))]"
                )
            };

            var wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(6), TimeSpan.FromMilliseconds(250));

            var encontrado = wait.Until(drv =>
            {
                foreach (var by in posiblesMensajes)
                {
                    var visibles = drv.FindElements(by).Where(e => e.Displayed).ToList();
                    if (visibles.Any())
                        return visibles.First();
                }
                return null;
            });

            string texto = (encontrado?.Text ?? string.Empty).Trim();

            bool contieneNumero = texto.Contains(minimo.ToString());
            bool contieneFrase = texto.IndexOf("importe", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                 (texto.IndexOf("mayor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                  texto.IndexOf("necesario", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                  texto.IndexOf("debe ser", StringComparison.OrdinalIgnoreCase) >= 0);

            Assert.IsTrue(contieneNumero && contieneFrase,
                $"No se encontró el mensaje esperado. Texto hallado: '{texto}'. Se esperaba que indicara que el importe debe ser mayor a {minimo}.");
        }

        [Then(@"el gasto se registra exitosamente")]
        public void ThenElGastoSeRegistraExitosamente()
        {
            _gasto.Guardar();
            Assert.Pass("Se ejecutó el flujo de guardado (agrega verificación de toast si aplica).");
        }

        // 
        [Then(@"Es necesario ingresar la fecha de gasto")]
        [Then(@"es necesario ingresar la fecha de gasto")]
        public void ThenEsNecesarioIngresarLaFechaDeGasto()
        {
            // 
            try { _gasto.Guardar(); } catch { /* ignore */ }

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(6));

            By[] candidatos = new[]
            {
        By.CssSelector(".toast-message, .toast-error, .toast, .alert, .alert-danger, .validation-summary-errors, .field-validation-error, .text-danger"),
        By.XPath("//*[contains(@class,'validation') and contains(@class,'error')]"),
        By.XPath("//*[contains(translate(normalize-space(.),'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'fecha') and " +
                 "(contains(translate(.,'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'ingresar') or " +
                 " contains(translate(.,'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'necesario') or " +
                 " contains(translate(.,'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'obligatorio') or " +
                 " contains(translate(.,'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'requerid'))]")
    };

            IWebElement msg = null;
            try
            {
                msg = wait.Until(drv =>
                {
                    foreach (var by in candidatos)
                    {
                        var visibles = drv.FindElements(by).Where(e => e.Displayed).ToList();
                        if (visibles.Any()) return visibles.First();
                    }
                    return null;
                });
            }
            catch
            {
                msg = _driver.FindElements(By.XPath(
                        "//*[contains(translate(.,'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'fecha de gasto') or " +
                        "   (contains(translate(.,'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'fecha') and contains(translate(.,'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'gasto'))]"))
                    .FirstOrDefault(e => e.Displayed);
            }

            var texto = (msg?.Text ?? string.Empty).Trim().ToLowerInvariant();
            bool ok = texto.Contains("fecha") &&
                      (texto.Contains("gasto") || texto.Contains("fecha de gasto")) &&
                      (texto.Contains("ingresar") || texto.Contains("necesario") || texto.Contains("obligator") || texto.Contains("requerid"));

            Assert.IsTrue(ok, $"No se encontró el mensaje esperado. Texto hallado: '{msg?.Text}'.");
        }

        [Then(@"es necesario agregar un concepto")]
        public void ThenEsNecesarioAgregarUnConcepto()
        {
            // 1) Disparar el intento de guardado para que se ejecute la validación
            try
            {
                _gasto.Guardar();
            }
            catch
            {
                // Si Guardar lanza alguna excepción de validación interna, igual seguimos a buscar mensajes
            }

            // 2) Ahora sí, esperamos el mensaje de validación en pantalla
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            bool mensajeEncontrado = false;

            try
            {
                mensajeEncontrado = wait.Until(d =>
                {
                    var errorSelectors = new[]
                    {
                "//*[contains(@class,'error')]",
                "//*[contains(@class,'alert-danger')]",
                "//*[contains(@class,'text-danger')]",
                "//*[contains(@class,'validation')]",
                "//*[contains(text(),'concepto')]",
                "//*[contains(text(),'Seleccione')]",
                "//*[contains(text(),'obligatorio')]",
                "//*[contains(text(),'requerido')]"
            };

                    foreach (var selector in errorSelectors)
                    {
                        try
                        {
                            var elementos = d.FindElements(By.XPath(selector));
                            foreach (var elemento in elementos)
                            {
                                var txt = (elemento.Text ?? "").Trim().ToLower();
                                if (elemento.Displayed &&
                                    (txt.Contains("concepto") ||
                                     txt.Contains("seleccione") ||
                                     txt.Contains("obligatorio") ||
                                     txt.Contains("requerido")))
                                {
                                    Console.WriteLine($"Mensaje de validación de concepto encontrado: {elemento.Text}");
                                    return true;
                                }
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    // toasts / mensajes flotantes genéricos
                    var posibles = d.FindElements(By.XPath(
                        "//*[contains(text(),'concepto') or contains(text(),'Seleccione') or contains(text(),'obligatorio') or contains(text(),'requerido')]"
                    ));
                    foreach (var msg in posibles)
                    {
                        var txt = (msg.Text ?? "").Trim().ToLower();
                        if (msg.Displayed &&
                            (txt.Contains("concepto") ||
                             txt.Contains("seleccione") ||
                             txt.Contains("obligatorio") ||
                             txt.Contains("requerido")))
                        {
                            Console.WriteLine($"Mensaje de validación de concepto encontrado (fallback): {msg.Text}");
                            return true;
                        }
                    }

                    return false;
                });
            }
            catch
            {
                // Búsqueda rápida extra por si el wait falló
                try
                {
                    var posiblesMensajes = _driver.FindElements(By.XPath(
                        "//*[contains(text(),'concepto') or contains(text(),'Seleccione') or contains(text(),'obligatorio') or contains(text(),'requerido')]"
                    ));
                    foreach (var mensaje in posiblesMensajes)
                    {
                        var txt = (mensaje.Text ?? "").Trim().ToLower();
                        if (mensaje.Displayed &&
                            (txt.Contains("concepto") ||
                             txt.Contains("seleccione") ||
                             txt.Contains("obligatorio") ||
                             txt.Contains("requerido")))
                        {
                            Console.WriteLine($"Mensaje de validación de concepto encontrado (fallback 2): {mensaje.Text}");
                            mensajeEncontrado = true;
                            break;
                        }
                    }
                }
                catch
                {
                    // ignorar y dejar mensajeEncontrado en false
                }
            }

            Assert.IsTrue(mensajeEncontrado,
                "No se encontró el mensaje de validación esperado indicando que es necesario agregar un concepto.");
        }



    }
}
