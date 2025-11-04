using AutomatizacionPOM.Pages.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace AutomatizacionPOM.Pages
{
    public class NewCotization
    {
        private readonly IWebDriver driver;
        private readonly Utilities utilities;

        public NewCotization(IWebDriver driver)
        {
            this.driver = driver;
            utilities = new Utilities(driver);
        }

        //        LOCALIZADORES

        // Concepto
        public static readonly By ConceptInputAlternative1 = By.CssSelector("selector-concepto-comercial input");
        public static readonly By selConceptSelection = By.XPath("//input[@placeholder='Concepto']");

        // IGV
        public static readonly By IgxTextContainer = By.XPath("//*[contains(translate(normalize-space(.),'ÁÉÍÓÚ','AEIOU'),'COTIZACION CON IGV')][descendant::input[@type='checkbox']]");
        public static readonly By IgxInputInside = By.XPath("(//*[contains(translate(normalize-space(.),'ÁÉÍÓÚ','AEIOU'),'COTIZACION CON IGV')]//input[@type='checkbox'])[1]");
        public static readonly By IgxById = By.CssSelector("#cotizacionigv");
        public static readonly By IgxLabelOrSpan = By.XPath("(//*[contains(translate(normalize-space(.),'ÁÉÍÓÚ','AEIOU'),'COTIZACION CON IGV')]//label|//*[contains(translate(normalize-space(.),'ÁÉÍÓÚ','AEIOU'),'COTIZACION CON IGV')]//span)[1]"
        );

        // Modal (si aplica)
        public static readonly By NuevaCotizacionModal = By.XPath("//div[contains(@class,'modal') and contains(@class,'show')]");

        // Documento (DNI/RUC)
        public static readonly By IdCustomer = By.Id("DocumentoIdentidad");
        public static readonly By CustomerSearchButton = By.XPath("//input[@id='DocumentoIdentidad']/following::button[1]");
        public static readonly By CustomerResolvedName = By.XPath("//input[@id='DocumentoIdentidad']/following::*[self::div or self::span or self::p][1]");
        public static readonly By AnyVariosTextNearCustomer = By.XPath("//*[contains(normalize-space(.),'VARIOS') or contains(normalize-space(.),'Varios')]");

        // Buscador (modal) de clientes
        public static readonly By CustomerSearchModal = By.XPath("//div[contains(@class,'modal') and contains(@class,'show')]");
        public static readonly By CustomerResultsTable = By.XPath("//table[contains(@class,'table') or contains(@class,'tabla')]");
        public static readonly By CustomerResultsRows = By.XPath("//table//tbody//tr");
        public static readonly By CustomerSelectButtonInRow = By.XPath(".//button[contains(.,'Seleccionar') or contains(.,'SELECCIONAR') or @title='Seleccionar']");
        public static readonly By CustomerDocCellInRow = By.XPath(".//td[contains(.,'DNI') or contains(.,'RUC') or .//span or .//div]");

        // Otros campos
        public static readonly By ConceptAmount = By.Id("cantidad-0");
        // Cualquier input de cantidad (cantidad-0, cantidad-1, ...)
        public static readonly By AnyQuantityInput = By.CssSelector("input[id^='cantidad-']");
        // Alias robusto: input inmediatamente después del label “ALIAS”
        public static readonly By Alias = By.XPath("(//label[contains(translate(normalize-space(.),'áéíóúÁÉÍÓÚ','aeiouAEIOU'),'ALIAS')]/following::input[1])[1]");
        public static readonly By FechaVencimiento = By.Id("fechaVencimiento");
        public static readonly By SaveSaleButton = By.XPath("//button[@title='GUARDAR COTIZACIÓN']");

        // =========================
        //         MÉTODOS
        // =========================

        // ---------- Concepto ----------
        public void SelectConcept(string codeconcept)
        {
            if (string.IsNullOrWhiteSpace(codeconcept) || codeconcept.Equals("ninguno", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Concepto omitido (valor 'ninguno') → no habrá filas de detalle; el total quedará en 0.");
                return;
            }

            Console.WriteLine($"=== INGRESANDO CONCEPTO: {codeconcept} ===");
            var locators = new[] { ConceptInputAlternative1, selConceptSelection };

            foreach (var locator in locators)
            {
                try
                {
                    Console.WriteLine($"Probando locator: {locator}");
                    EnterConceptWithEnter(locator, codeconcept);
                    Console.WriteLine($"¡Éxito ingresando concepto con locator: {locator}!");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Locator {locator} falló: {ex.Message}");
                }
            }

            throw new InvalidOperationException($"No se pudo ingresar el concepto '{codeconcept}' con ningún localizador");
        }

        private void EnterConceptWithEnter(By locator, string concept)
        {
            var element = utilities.WaitForElementVisible(locator);
            utilities.ScrollViewElement(element);

            element.Clear();
            Thread.Sleep(200);
            element.SendKeys(concept);
            Thread.Sleep(500);
            element.SendKeys(Keys.Enter);
            Thread.Sleep(250);

            Console.WriteLine($"Concepto '{concept}' ingresado exitosamente");
        }

        // ---------- Cantidad (tolerante cuando no hay ítems) ----------
        public void EnterAmount(string amount)
        {
            Console.WriteLine($"Ingresando cantidad: {amount}");

            // Si no hay ningún campo de cantidad visible, no hay productos (caso 'ninguno')
            var qtyInputs = driver.FindElements(AnyQuantityInput);
            if (qtyInputs == null || qtyInputs.Count == 0)
            {
                Console.WriteLine("No hay campo de cantidad visible (no hay productos en el carrito). Se continúa sin ingresar cantidad.");
                return; // el total quedará en 0 y se validará el mensaje esperado
            }

            // Toma el primero visible
            IWebElement qty = null;
            foreach (var candidate in qtyInputs)
            {
                if (candidate.Displayed && candidate.Enabled) { qty = candidate; break; }
            }

            if (qty == null)
            {
                Console.WriteLine("No se encontró un campo de cantidad interactuable. Se continúa sin ingresar cantidad.");
                return;
            }

            utilities.ScrollViewElement(qty);
            qty.Clear();
            Thread.Sleep(80);
            qty.SendKeys(amount);
            Thread.Sleep(80);

            // Dispara eventos + blur para recalcular totales
            try
            {
                var js = (IJavaScriptExecutor)driver;
                js.ExecuteScript(@"
                    (function(el){
                        el.dispatchEvent(new Event('input', {bubbles:true}));
                        el.dispatchEvent(new Event('change', {bubbles:true}));
                    })(arguments[0]);", qty);
            }
            catch { /* ignore */ }

            qty.SendKeys(Keys.Tab);
            Thread.Sleep(100);

            Console.WriteLine("Cantidad ingresada (o tolerada sin campo) correctamente.");
        }

        // ---------- IGV ----------
        public void SetIGV(bool on)
        {
            Console.WriteLine($"=== Asegurar IGV = {(on ? "ON" : "OFF")} ===");

            TrySilent(() => WaitShort(NuevaCotizacionModal, 5)); // no bloquea si no hay modal

            var strategies = new Func<bool>[]
            {
                () => TryEnsureState(() => {
                    var c = WaitShort(IgxTextContainer, 8);
                    var input = c.FindElement(By.XPath(".//input[@type='checkbox']"));
                    return EnsureCheckboxState(input, on);
                }),
                () => TryEnsureState(() => {
                    var input = WaitShort(IgxById, 5);
                    return EnsureCheckboxState(input, on);
                }),
                () => TryEnsureState(() => {
                    var input = WaitShort(IgxInputInside, 5);
                    return EnsureCheckboxState(input, on);
                }),
                () => TryEnsureState(() => {
                    var lbl = WaitShort(IgxLabelOrSpan, 5);
                    utilities.ScrollViewElement(lbl);
                    try { lbl.Click(); } catch { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", lbl); }
                    Thread.Sleep(200);

                    IWebElement input;
                    try { input = lbl.FindElement(By.XPath(".//preceding::input[@type='checkbox'][1] | .//following::input[@type='checkbox'][1]")); }
                    catch { input = driver.FindElement(IgxById); }
                    return input != null && input.Selected == on;
                })
            };

            foreach (var s in strategies)
                if (s()) { Console.WriteLine("✅ IGV OK"); return; }

            throw new InvalidOperationException("No se pudo asegurar el estado del checkbox IGV");
        }

        private bool EnsureCheckboxState(IWebElement input, bool desired)
        {
            utilities.ScrollViewElement(input);
            if (input.Selected == desired) return true;

            try { input.Click(); }
            catch { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", input); }

            Thread.Sleep(150);
            if (input.Selected != desired)
            {
                input.SendKeys(Keys.Space);
                Thread.Sleep(120);
            }
            if (input.Selected != desired)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("document.querySelector('#cotizacionigv')?.click();");
                Thread.Sleep(120);
            }
            return input.Selected == desired;
        }

        private bool TryEnsureState(Func<bool> attempt)
        {
            try { return attempt(); }
            catch (Exception ex) { Console.WriteLine($"Estrategia IGV falló: {ex.Message}"); return false; }
        }

        // ---------- Cliente: DNI/RUC ----------
        public void SetCustomerByDocument(string document, string expectedNameOrRazonSocial = null, int timeoutSeconds = 12, bool allowVarios = false)
        {
            string digits = Regex.Replace(document ?? "", "[^0-9]", "");
            if (digits.Length != 8 && digits.Length != 11)
                throw new ArgumentException($"Documento inválido '{document}'. Use DNI (8) o RUC (11).");

            bool isRuc = digits.Length == 11;
            Console.WriteLine($"[CLIENTE] Ingresando {(isRuc ? "RUC" : "DNI")}: {digits}");

            var input = utilities.WaitForElementVisible(IdCustomer);
            utilities.ScrollViewElement(input);
            input.Clear();
            Thread.Sleep(80);
            input.SendKeys(digits);

            TriggerLookup(input);

            if (allowVarios && IsCurrentlyVarios())
            {
                Console.WriteLine("[CLIENTE] Estado 'VARIOS' aceptado por escenario ");
                return;
            }

            if (WaitUntilCustomerResolved(expectedNameOrRazonSocial, timeoutSeconds))
            {
                Console.WriteLine("[CLIENTE] Resolución por Enter/JS OK ");
                return;
            }

            if (!allowVarios)
            {
                Console.WriteLine("[CLIENTE] Fallback: botón lupa...");
                TrySilent(() =>
                {
                    var btn = WaitShort(CustomerSearchButton, 5);
                    utilities.ScrollViewElement(btn);
                    try { btn.Click(); } catch { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn); }
                    Thread.Sleep(400);
                });

                if (!WaitUntilCustomerResolved(expectedNameOrRazonSocial, timeoutSeconds))
                    throw new InvalidOperationException("El cliente no se resolvió (sigue 'VARIOS' o no apareció el nombre/razón social esperado).");
            }
            else
            {
                Console.WriteLine("[CLIENTE] allowVarios=true: continuamos con 'VARIOS'.");
            }
        }

        // Buscar en modal por documento exacto
        public void EnsureCustomerResolvedFromSearchByDocument(string document, int timeoutSeconds = 12)
        {
            if (WaitUntilCustomerResolved(null, 1)) return;

            Console.WriteLine("[CLIENTE] Desambiguando VARIOS vía modal de búsqueda...");

            var btn = WaitShort(CustomerSearchButton, 5);
            utilities.ScrollViewElement(btn);
            try { btn.Click(); } catch { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn); }

            var modal = WaitShort(CustomerSearchModal, 6);
            var table = WaitShort(CustomerResultsTable, 6);
            utilities.ScrollViewElement(table);

            string target = Regex.Replace(document ?? "", "[^0-9]", "");
            bool selected = false;

            var rows = driver.FindElements(CustomerResultsRows);
            foreach (var row in rows)
            {
                try
                {
                    var docCell = row.FindElement(CustomerDocCellInRow);
                    var raw = (docCell.Text ?? string.Empty);
                    string digits = Regex.Replace(raw, "[^0-9]", "");
                    if (!string.IsNullOrEmpty(digits) && digits.Equals(target))
                    {
                        var selectBtn = row.FindElement(CustomerSelectButtonInRow);
                        utilities.ScrollViewElement(selectBtn);
                        try { selectBtn.Click(); }
                        catch { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", selectBtn); }
                        selected = true;
                        break;
                    }
                }
                catch { /* ignorar fila */ }
            }

            if (!selected)
                throw new InvalidOperationException($"No se encontró en resultados un cliente con documento {target}");

            if (!WaitUntilCustomerResolved(null, timeoutSeconds))
                throw new InvalidOperationException("Tras seleccionar en el modal, el cliente sigue como 'VARIOS'.");
        }

        private void TriggerLookup(IWebElement input)
        {
            try
            {
                input.SendKeys(Keys.Enter);
                Thread.Sleep(80);

                var js = (IJavaScriptExecutor)driver;
                string script = @"
                    (function(el){
                        function k(t){return new KeyboardEvent(t,{bubbles:true,cancelable:true,key:'Enter',code:'Enter',keyCode:13,which:13});}
                        el.dispatchEvent(new Event('input',{bubbles:true}));
                        el.dispatchEvent(new Event('change',{bubbles:true}));
                        el.dispatchEvent(k('keydown'));
                        el.dispatchEvent(k('keypress'));
                        el.dispatchEvent(k('keyup'));
                    })(arguments[0]);";
                js.ExecuteScript(script, input);

                input.SendKeys(Keys.Tab);
                Thread.Sleep(120);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENTE] Error disparando eventos: {ex.Message}");
            }
        }

        private bool WaitUntilCustomerResolved(string expectedNameOrRazonSocial, int timeoutSeconds)
        {
            DateTime end = DateTime.UtcNow.AddSeconds(timeoutSeconds);

            while (DateTime.UtcNow < end)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(expectedNameOrRazonSocial))
                    {
                        string needle = expectedNameOrRazonSocial.Trim().ToLowerInvariant();
                        var found = driver.FindElements(By.XPath(
                            $"//*[contains(translate(normalize-space(.),'ÁÉÍÓÚáéíóú','AEIOUaeiou'), \"{needle}\")]"
                        ));
                        if (found.Count > 0) return true;
                    }
                    else
                    {
                        var near = driver.FindElements(CustomerResolvedName);
                        foreach (var e in near)
                        {
                            var txt = (e.Text ?? "").Trim();
                            if (!string.IsNullOrEmpty(txt) &&
                                !txt.Equals("VARIOS", StringComparison.OrdinalIgnoreCase) &&
                                txt.Length >= 3)
                                return true;
                        }

                        var varios = driver.FindElements(AnyVariosTextNearCustomer);
                        if (varios.Count == 0) return true;
                    }
                }
                catch { /* seguir reintentando */ }

                Thread.Sleep(200);
            }
            return false;
        }

        private bool IsCurrentlyVarios()
        {
            try
            {
                var varios = driver.FindElements(AnyVariosTextNearCustomer);
                return varios.Count > 0;
            }
            catch { return false; }
        }

        // ---------- Alias ----------
        public void EnterAlias(string alias)
        {
            var el = utilities.WaitForElementVisible(Alias);
            utilities.ScrollViewElement(el);
            el.Click();
            el.Clear();
            Thread.Sleep(60);
            el.SendKeys(alias);

            try
            {
                var js = (IJavaScriptExecutor)driver;
                js.ExecuteScript(@"
                    (function(el){
                        el.dispatchEvent(new Event('input', {bubbles:true}));
                        el.dispatchEvent(new Event('change', {bubbles:true}));
                    })(arguments[0]);", el);
            }
            catch { /* ignore */ }

            el.SendKeys(Keys.Tab);
            Thread.Sleep(100);
        }

        /// <summary>
        /// Regla única: si 'documentoValor' es DNI/RUC (8/11 dígitos exactos) -> busca cliente;
        /// si NO es numérico -> fuerza VARIOS y escribe ese valor como ALIAS (literal del caso).
        /// </summary>
        public void SetCustomerOrAliasFromCase(string documentoValor)
        {
            if (string.IsNullOrWhiteSpace(documentoValor))
                return;

            var digits = Regex.Replace(documentoValor, "[^0-9]", "");
            bool esNumeroValido = (digits.Length == 8 || digits.Length == 11) && digits == documentoValor;

            if (esNumeroValido)
            {
                bool allowVarios = documentoValor == "00000000";
                SetCustomerByDocument(documentoValor, null, 12, allowVarios);
                return; // alias se oculta; no hacer nada más
            }

            // No es numérico -> usarlo como ALIAS
            SetUnknownCustomerAlias();          // deja “VARIOS”
            EnterAlias(documentoValor.Trim());  // alias literal del caso
        }

        // Prepara estado base VARIOS (limpiando el documento y blureando)
        public void SetUnknownCustomerAlias()
        {
            var doc = utilities.WaitForElementVisible(IdCustomer);
            utilities.ScrollViewElement(doc);
            doc.Clear();
            Thread.Sleep(100);
            doc.SendKeys(Keys.Tab); // la UI pasa a "VARIOS"
            Thread.Sleep(120);
            Console.WriteLine("[CLIENTE] Campo documento limpiado → estado base 'VARIOS'.");
        }

        // ---------- Otros ----------
        public void EnterFechaVencimiento(string fecha)
        {
            Console.WriteLine($"Ingresando fecha de vencimiento: {fecha}");
            utilities.ClearAndEnterText(FechaVencimiento, fecha);
        }

        public void SaveCotization()
        {
            Console.WriteLine("Guardando cotización...");
            try
            {
                utilities.ClickButton(SaveSaleButton);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SAVE] Click interceptado, intentando JS: {ex.Message}");
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", driver.FindElement(SaveSaleButton));
            }
            Thread.Sleep(800);
        }

        public bool IsMensajePresente(string mensaje)
        {
            try
            {
                var elementos = driver.FindElements(By.XPath($"//*[contains(text(), '{mensaje}')]"));
                return elementos.Count > 0;
            }
            catch { return false; }
        }

        // =========================
        //     UTILIDADES LOCALES
        // =========================

        private IWebElement WaitShort(By by, int seconds = 8)
        {
            DateTime end = DateTime.UtcNow.AddSeconds(seconds);
            Exception last = null;

            while (DateTime.UtcNow < end)
            {
                try
                {
                    var el = driver.FindElement(by);
                    if (el.Displayed) return el;
                }
                catch (Exception ex) { last = ex; }
                Thread.Sleep(120);
            }
            throw last ?? new WebDriverTimeoutException($"Timeout esperando visible: {by}");
        }

        private void TrySilent(Action act)
        {
            try { act(); } catch (Exception ex) { Console.WriteLine($"(ignorado) {ex.Message}"); }
        }
    }
}
