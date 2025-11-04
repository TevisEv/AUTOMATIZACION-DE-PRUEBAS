using AutomatizacionPOM.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using System;

namespace AutomatizacionPOM.StepDefinitions
{
    [Binding]
    public class NewCotizationStepDefinitions
    {
        private readonly IWebDriver _driver;
        private readonly NewCotization _newCotization;
        private readonly ScenarioContext _scenarioContext;

        public NewCotizationStepDefinitions(IWebDriver driver, ScenarioContext scenarioContext)
        {
            _driver = driver;
            _scenarioContext = scenarioContext;
            _newCotization = new NewCotization(driver);
        }

        // ------------------ Concepto ------------------

        [When(@"el usuario agrega el concepto '(.*)'")]
        public void WhenElUsuarioAgregaElConcepto(string concepto)
        {
            try
            {
                Console.WriteLine($"=== INICIANDO: Agregar concepto '{concepto}' ===");
                _newCotization.SelectConcept(concepto);
                Console.WriteLine($"=== COMPLETADO: Concepto '{concepto}' agregado ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR: No se pudo agregar concepto '{concepto}' - {ex.Message} ===");
                throw;
            }
        }

        [When(@"ingresa la cantidad '(.*)'")]
        public void WhenIngresaLaCantidad(string cantidad)
        {
            Console.WriteLine($"Ingresando cantidad: {cantidad}");
            _newCotization.EnterAmount(cantidad);
        }

        // ------------------ IGV ------------------

        [When(@"selecciona igv")]
        public void WhenSeleccionaIgv()
        {
            Console.WriteLine("Seleccionando IGV");
            _newCotization.SetIGV(true);
        }

        [When(@"no selecciona igv")]
        public void WhenNoSeleccionaIgv()
        {
            Console.WriteLine("Dejando IGV desmarcado");
            _newCotization.SetIGV(false);
        }

        // ------------------ Cliente / Alias (regla única) ------------------

        [When(@"selecciona al cliente con documento '(.*)'")]
        public void WhenSeleccionaAlClienteConDocumento(string documento)
        {
            Console.WriteLine($"[CLIENTE] Valor recibido en el caso: '{documento}'");
            _newCotization.SetCustomerOrAliasFromCase(documento);
        }

        // ------------------ Fecha de vencimiento ------------------

        [When(@"ingresa la fecha de vencimiento '(.*)'")]
        public void WhenIngresaLaFechaDeVencimiento(string fecha)
        {
            Console.WriteLine($"Ingresando fecha de vencimiento: {fecha}");
            _newCotization.EnterFechaVencimiento(fecha);
        }

        // ------------------ Guardar y verificaciones ------------------

        [Then(@"la cotizacion se guarda correctamente")]
        public void ThenLaCotizacionSeGuardaCorrectamente()
        {
            Console.WriteLine("Verificando que la cotización se guarda correctamente...");
            _newCotization.SaveCotization();

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(8));
            bool ok = false;

            try
            {
                ok = wait.Until(d =>
                    d.Url.ToLowerInvariant().Contains("cotizacion") || d.Url.ToLowerInvariant().Contains("index")
                    || _newCotization.IsMensajePresente("éxito")
                    || _newCotization.IsMensajePresente("guardó")
                    || _newCotization.IsMensajePresente("confirmado")
                    || _newCotization.IsMensajePresente("se guardó")
                );
            }
            catch { /* caerá en el Assert */ }

            Assert.IsTrue(ok, "La cotización no se guardó correctamente (no cambió la vista ni apareció mensaje de éxito).");
        }

        [Then(@"el sistema muestra mensaje de error")]
        public void ThenElSistemaMuestraMensajeDeError()
        {
            _newCotization.SaveCotization();

            bool errorShown =
                _newCotization.IsMensajePresente("error")
                || _newCotization.IsMensajePresente("no se pudo")
                || _newCotization.IsMensajePresente("incompleto")
                || _newCotization.IsMensajePresente("obligatorio");

            Assert.IsTrue(errorShown, "Se esperaba un error pero no se mostró mensaje de error.");
        }

        [Then(@"el sistema muestra mensaje: '(.*)'")]
        public void ThenElSistemaMuestraMensaje(string mensajeEsperado)
        {
            try { _newCotization.SaveCotization(); } catch { /* puede lanzar; igual verificamos */ }
            Assert.IsTrue(_newCotization.IsMensajePresente(mensajeEsperado),
                $"No se encontró el mensaje esperado: '{mensajeEsperado}'");
        }
    }
}
