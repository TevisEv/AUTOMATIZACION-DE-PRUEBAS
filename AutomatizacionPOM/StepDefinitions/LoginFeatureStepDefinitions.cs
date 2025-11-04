using AutomatizacionPOM.Pages;
using OpenQA.Selenium;
using Reqnroll;

namespace AutomatizacionPOM.StepDefinitions
{
    [Binding]
    public class LoginFeatureStepDefinitions
    {
        private readonly IWebDriver driver;
        private readonly AccessPage accessPage;

        public LoginFeatureStepDefinitions(IWebDriver driver)
        {
            this.driver = driver;
            accessPage = new AccessPage(driver);
        }

        [Given("el usuario ingresa al ambiente {string}")]
        public void GivenElUsuarioIngresaAlAmbiente(string ambiente)
        {
            accessPage.OpenToAplicattion(ambiente);
        }

        [When("el usuario inicia sesión con usuario {string} y contraseña {string}")]
        public void WhenElUsuarioIniciaSesionConUsuarioYContrasena(string user, string password)
        {
            accessPage.LoginToApplication(user, password);
        }

        [When("accede al módulo {string}")]
        public void WhenAccedeAlModulo(string modulo)
        {
            accessPage.enterModulo(modulo);
        }

        // acciones internas como “NUEVO GASTO”, “NUEVA COTIZACIÓN”
        [When("accede al submódulo {string}")]
        public void WhenAccedeAlSubmodulo(string submodulo)
        {
            accessPage.enterSubModulo(submodulo);
        }

        // PASOS PARA EL MENÚ LATERAL DE GASTO 

        [When("accede al submoudo {string}")]
        public void WhenAccedeAlSubmoudo(string submenulateral)
        {
            accessPage.enterSubmenuGasto(submenulateral); // “Ver”, “Concepto”, “Reporte”
        }

        [When("accede al submenú {string}")]
        public void WhenAccedeAlSubmenu(string submenulateral)
        {
            accessPage.enterSubmenuGasto(submenulateral);
        }
    }
}
