using AutomatizacionPOM.Pages.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AutomatizacionPOM.Pages
{
    public class Gasto
    {
        private readonly IWebDriver driver;
        private readonly Utilities utilities;

        public Gasto(IWebDriver driver)
        {
            this.driver = driver;
            utilities = new Utilities(driver);
        }

        // ──────────────────────────────────────────────────────────────
        // LOCATORS PRINCIPALES
        // ──────────────────────────────────────────────────────────────
        private readonly By Documento = By.Id("DocumentoIdentidad");
        private readonly By FechaGasto = By.Id("fechaRegistro");
        private readonly By Observaciones = By.Id("observacion");

        // Detalle
        private readonly By DetalleId = By.Id("detalle");
        private readonly By DetalleName = By.Name("detalle");
        private readonly By DetalleNgModel = By.CssSelector("textarea[ng-model*='detalle'], textarea[ng-model*='Detalle']");
        private readonly By DetalleAbsolute = By.XPath("/html/body/div[4]/div/div/div/div[2]/div[1]/form//textarea[@id='detalle']");

        // Concepto (Select2 + nativos + inputs planos)
        private readonly By ConceptoLabelBlock = By.XPath(
            "//*[self::label or self::span or self::div][contains(translate(normalize-space(.),'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'concepto')]"
        );

        // Contenedores Select2 alrededor del área de Concepto
        private readonly By ConceptoS2Rendered = By.CssSelector("span.select2-selection__rendered");
        private readonly By ConceptoS2Single = By.CssSelector("span.select2-selection--single");
        private readonly By ConceptoS2Any = By.CssSelector("span.select2, span.select2-container");

        private readonly By Select2SearchInput = By.CssSelector("input.select2-search__field");
        private readonly By Select2ResultsAny = By.CssSelector(".select2-results__option");

        // Select nativo / input plano
        private readonly By ConceptoNativeSelect = By.CssSelector("select#concepto, select[name='concepto'], select[ng-model*='Concepto'], select[ng-model*='concepto']");
        private readonly By ConceptoPlainInput = By.CssSelector("input#concepto, input[name='concepto'], input[placeholder*='concepto' i], input[ng-model*='concepto']");

        // Importe
        private readonly By Importe = By.CssSelector("input[ng-model*='Importe'], #importe, [name='importe']");

        // Tipo de gasto
        private readonly By RadioRapido = By.Id("radio1");
        private readonly By RadioConfigurable = By.Id("radio2");
        private readonly By RadioContado = By.Id("radio0");
        private readonly By LabelConfigurableText = By.XPath("//*[self::label or self::span or self::div]" +
            "[contains(translate(normalize-space(.),'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'credito configurable') or " +
            " contains(translate(normalize-space(.),'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'gasto al credito configurable') or " +
            " contains(translate(normalize-space(.),'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'cr\u00E9dito configurable')]");

        // Crédito configurable
        private readonly By Inicial = By.Id("inicial");
        private readonly By Cuotas = By.Id("cuota");
        private readonly By DiaPago = By.Id("diavencimiento"); // select oculto de Select2 (NO clickear)
        private readonly By DiaPagoS2Item = By.XPath("//li[starts-with(@id,'select2-diavencimiento-result-')]");
        private readonly By BtnGenerarCuotas = By.XPath("//button[@title='Generar cuota(s)']");
        private readonly By AceptarCuotas = By.XPath("//button[normalize-space()='ACEPTAR']");
        private readonly By InputsConfigActivos = By.XPath("//input[@id='inicial' or @id='cuota' or contains(@ng-model,'Inicial') or contains(@ng-model,'Cuota')]");

        // >>> NUEVOS: Select2 de día de vencimiento (container visible y dropdown abierto)
        private readonly By DiaPagoS2Container = By.CssSelector("#select2-diavencimiento-container, [aria-labelledby='select2-diavencimiento-container']");
        private readonly By Select2OpenContainer = By.CssSelector("span.select2-container--open");

        // IGV
        private readonly By IGVById = By.Id("ventaigv");
        private readonly By IGVByText = By.XPath("//*[contains(translate(normalize-space(.),'ÁÉÍÓÚ','AEIOU'),'IGV')][descendant::input[@type='checkbox']]");

        // Guardar
        private readonly By BtnGuardar = By.XPath("//button[normalize-space()='GUARDAR']");

        // ──────────────────────────────────────────────────────────────
        // PASOS
        // ──────────────────────────────────────────────────────────────

        public void IngresarDocumento(string documento)
        {
            var el = utilities.WaitForElementVisible(Documento);
            utilities.ScrollViewElement(el);
            el.Clear();
            el.SendKeys(documento ?? "");
            SendEvents(el);
            el.SendKeys(Keys.Enter);
            Thread.Sleep(100);
            el.SendKeys(Keys.Tab);
        }

        public void IngresarFechaGasto(string fecha) => utilities.ClearAndEnterText(FechaGasto, fecha);
        public void IngresarObservaciones(string texto) => utilities.ClearAndEnterText(Observaciones, texto);

        public void IngresarDetalle(string detalle)
        {
            var el = FirstVisible(DetalleId, 1)
                     ?? FirstVisible(DetalleName, 1)
                     ?? FirstVisible(DetalleNgModel, 2)
                     ?? FirstVisible(DetalleAbsolute, 1);

            if (el == null) throw new NoSuchElementException("No se encontró el input de 'detalle'.");

            utilities.ScrollViewElement(el);
            el.Clear();
            Thread.Sleep(50);
            el.SendKeys(detalle ?? "");
            SendEvents(el);
            el.SendKeys(Keys.Tab);
        }

        public void IngresarConcepto(string concepto)
        {
            // Si viene vacío/null, no hacemos nada
            if (string.IsNullOrWhiteSpace(concepto)) return;

            // Caso especial: 'NINGUNO' = dejar sin concepto (limpiar selección)
            var norm = concepto.Trim().ToUpperInvariant();
            if (norm == "NINGUNO")
            {
                try
                {
                    // 1) Intentar limpiar el <select> real que está oculto por Select2
                    var sel = FirstPresent(ConceptoNativeSelect, 2);
                    if (sel != null)
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript(@"
                    var s = arguments[0];
                    if (!s) return;

                    // Vaciar valor del <select>
                    s.value = '';

                    // Si hay Select2 inicializado, resetearlo también
                    if (window.jQuery && jQuery.fn && jQuery.fn.select2)
                    {
                        try {
                            jQuery(s).val(null).trigger('change');
                        } catch(e){}
                    }
                    else
                    {
                        s.dispatchEvent(new Event('input',  { bubbles: true }));
                        s.dispatchEvent(new Event('change', { bubbles: true }));
                    }
                ", sel);
                    }
                    else
                    {
                        // 2) Fallback: trabajar desde el span.rendered de Select2
                        var rendered = FirstPresent(ConceptoS2Rendered, 2);
                        if (rendered != null)
                        {
                            ((IJavaScriptExecutor)driver).ExecuteScript(@"
                        var r = arguments[0];
                        if (!r) return;

                        var container = r.closest('.select2-container');
                        var selectEl = null;

                        if (container && container.previousElementSibling && container.previousElementSibling.tagName === 'SELECT')
                        {
                            selectEl = container.previousElementSibling;
                        }

                        if (selectEl)
                        {
                            selectEl.value = '';

                            if (window.jQuery && jQuery.fn && jQuery.fn.select2)
                            {
                                try {
                                    jQuery(selectEl).val(null).trigger('change');
                                } catch(e){}
                            }
                            else
                            {
                                selectEl.dispatchEvent(new Event('input',  { bubbles: true }));
                                selectEl.dispatchEvent(new Event('change', { bubbles: true }));
                            }
                        }

                        // Texto visual tipo placeholder
                        r.textContent = 'Seleccione';
                    ", rendered);
                        }
                    }
                }
                catch
                {
                    // Si algo falla, no queremos romper el flujo
                }

                // Importante: salimos aquí, NO seleccionamos nada
                return;
            }

            // ───────────────────────────────────────────────
            // LÓGICA NORMAL PARA UN CONCEPTO REAL
            // ───────────────────────────────────────────────

            // 1) intentar Select2 “cerca” de la etiqueta Concepto
            var label = FirstVisible(ConceptoLabelBlock, 1);
            IWebElement s2 = null;

            if (label != null)
            {
                // busca un select2 contiguo
                s2 = FindSibling(label, ConceptoS2Single)
                     ?? FindSibling(label, ConceptoS2Rendered)
                     ?? FindSibling(label, ConceptoS2Any);
            }

            // si no se halló, intenta globalmente (pantalla)
            s2 ??= FirstVisible(ConceptoS2Single, 1)
                   ?? FirstVisible(ConceptoS2Rendered, 1)
                   ?? FirstVisible(ConceptoS2Any, 1);

            if (s2 != null)
            {
                // abrir popup
                SafeClick(s2);
                Thread.Sleep(120);

                var search = FirstPresent(Select2SearchInput, 2);
                if (search == null)
                    throw new NoSuchElementException("No se encontró el input de búsqueda de Select2 para 'Concepto'.");

                search.Clear();
                search.SendKeys(concepto);
                Thread.Sleep(220);
                // selecciona 1ra coincidencia con Enter
                search.SendKeys(Keys.Enter);
                Thread.Sleep(150);
                return;
            }

            // 2) si no hay Select2, intenta SELECT nativo
            var selNative = FirstVisible(ConceptoNativeSelect, 1);
            if (selNative != null)
            {
                try
                {
                    var select = new SelectElement(selNative);
                    // intenta por el texto tal cual
                    TrySelectByTextFlexible(select, concepto);
                    return;
                }
                catch { /* cae a input plano */ }
            }

            // 3) input plano
            IWebElement inputPlano = null;
            if (label != null)
                inputPlano = FindSibling(label, ConceptoPlainInput);

            inputPlano ??= FirstVisible(ConceptoPlainInput, 1);
            if (inputPlano != null)
            {
                inputPlano.Clear();
                inputPlano.SendKeys(concepto);
                SendEvents(inputPlano);
                inputPlano.SendKeys(Keys.Tab);
                return;
            }

            // 4) último recurso: cualquier input de texto cerca del label
            if (label != null)
            {
                var txt = FindSibling(label, By.CssSelector("input[type='text']"));
                if (txt != null)
                {
                    txt.Clear();
                    txt.SendKeys(concepto);
                    SendEvents(txt);
                    txt.SendKeys(Keys.Tab);
                    return;
                }
            }

            throw new NoSuchElementException("No se encontró el contenedor de Select2 para 'Concepto'.");
        }


        public void IngresarImporte(string importe) => utilities.ClearAndEnterText(Importe, importe);

        public void SeleccionarTipoGasto(string tipo)
        {
            var t = (tipo ?? "").Trim().ToLower();
            if (t.Contains("configurable"))
            {
                if (!ForcePickCreditoConfigurable())
                    throw new WebDriverTimeoutException("No se pudo seleccionar 'gasto al crédito configurable' (#radio2).");
                WaitPresent(InputsConfigActivos, 3);
                return;
            }
            if (t.Contains("rapido") || t.Contains("rápido")) { ClickHard(RadioRapido, 3); return; }
            if (t.Contains("contado")) { ClickHard(RadioContado, 3); return; }
        }

        public void ConfigurarCreditoConfigurable(string inicial, string cuotas, string dia)
        {
            if (!string.IsNullOrWhiteSpace(inicial))
                utilities.ClearAndEnterText(Inicial, inicial);

            if (!string.IsNullOrWhiteSpace(cuotas))
                utilities.ClearAndEnterText(Cuotas, cuotas);

            // === FIX: Selección robusta de día de pago (Select2) por texto visible ===
            if (!string.IsNullOrWhiteSpace(dia))
            {
                SelectDiaPago(dia.Trim());
            }

            // Generar cuotas (si corresponde)
            var gen = FirstVisible(BtnGenerarCuotas, 1);
            if (gen != null)
            {
                SafeClick(gen);
                Thread.Sleep(250);
                var AceptarCuota = FirstVisible(AceptarCuotas, 2);
                if (AceptarCuota != null) SafeClick(AceptarCuota);
                Thread.Sleep(150);
            }
        }

        public void SetIGV(bool conIgv)
        {
            var input = FirstPresent(IGVById, 1);
            if (input != null) { EnsureCheckboxState(input, conIgv); return; }

            var block = FirstVisible(IGVByText, 2);
            if (block != null)
            {
                var cb = block.FindElement(By.XPath(".//input[@type='checkbox']"));
                EnsureCheckboxState(cb, conIgv);
                return;
            }

            throw new NoSuchElementException("No se encontró el checkbox de IGV.");
        }

        public void Guardar()
        {
            var btn = FirstVisible(BtnGuardar, 2);
            if (btn == null) throw new NoSuchElementException("No se encontró el botón GUARDAR.");
            SafeClick(btn);
            Thread.Sleep(600);
        }

        // ──────────────────────────────────────────────────────────────
        // CORE: Selección robusta de #radio2 (configurable)
        // ──────────────────────────────────────────────────────────────
        private bool ForcePickCreditoConfigurable()
        {
            var el = FirstPresent(RadioConfigurable, 2);
            if (el != null && TryCheckRadio(el)) return true;

            var lbl = FirstPresent(LabelConfigurableText, 2);
            if (lbl != null)
            {
                JsClick(lbl);
                Thread.Sleep(50);

                var forId = lbl.GetAttribute("for");
                if (!string.IsNullOrEmpty(forId))
                {
                    var byFor = By.Id(forId);
                    el = FirstPresent(byFor, 1);
                    if (el != null && TryCheckRadio(el)) return true;
                }

                el = FirstPresent(RadioConfigurable, 1);
                if (el != null && TryCheckRadio(el)) return true;
            }

            try
            {
                var jsEl = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return document.querySelector('#radio2');");
                if (jsEl != null && TryCheckRadio(jsEl)) return true;
            }
            catch { }

            return false;
        }

        private bool TryCheckRadio(IWebElement radio)
        {
            try { radio.Click(); } catch { JsClick(radio); }
            Thread.Sleep(60);
            if (radio.Selected) return true;

            ((IJavaScriptExecutor)driver).ExecuteScript(@"
                var r = arguments[0];
                r.checked = true;
                r.dispatchEvent(new Event('input',  {bubbles:true}));
                r.dispatchEvent(new Event('change', {bubbles:true}));
                r.dispatchEvent(new MouseEvent('click', {bubbles:true}));
            ", radio);
            Thread.Sleep(80);
            return radio.Selected;
        }

        // ──────────────────────────────────────────────────────────────
        // NUEVOS HELPERS: Select2 Día de Pago
        // ──────────────────────────────────────────────────────────────

        // Abre el Select2 del día de pago clickeando el container visible
        private void OpenSelect2(By container)
        {
            var cont = FirstVisible(container, 3);
            if (cont == null)
                throw new NoSuchElementException("No se encontró el contenedor visible de Select2 para 'Día de pago'.");

            SafeClick(cont);

            // Espera a que el dropdown de Select2 se abra
            var opened = FirstVisible(Select2OpenContainer, 3);
            if (opened == null)
                throw new WebDriverTimeoutException("No se abrió el dropdown de Select2 para 'Día de pago'.");
        }

        // Selecciona una opción por texto visible exacto dentro del dropdown abierto
        private void PickSelect2OptionByVisibleText(string visibleTextExact)
        {
            // Si existe caja de búsqueda, úsala (depende de configuración de Select2)
            var search = FirstPresent(Select2SearchInput, 1);
            if (search != null && search.Displayed)
            {
                search.Clear();
                search.SendKeys(visibleTextExact);
                Thread.Sleep(180);
            }

            // Buscar la opción (texto exacto normalizado)
            var optionXpath = $"//span[contains(@class,'select2-container--open')]//li[contains(@class,'select2-results__option') and normalize-space()='{visibleTextExact}']";
            var li = FirstVisible(By.XPath(optionXpath), 2);

            // Si no aparece por filtrado, cae a lista general y busca el mejor match
            li ??= FirstVisible(By.XPath("//span[contains(@class,'select2-container--open')]//li[contains(@class,'select2-results__option') and not(contains(@class,'loading-results'))]"), 2);

            if (li == null)
                throw new NoSuchElementException($"No se encontró la opción de Select2 con texto '{visibleTextExact}'.");

            SafeClick(li);

            // Esperar cierre del dropdown (estabilidad)
            var end = DateTime.UtcNow.AddSeconds(2);
            while (DateTime.UtcNow < end)
            {
                var stillOpen = driver.FindElements(Select2OpenContainer).Count > 0;
                if (!stillOpen) break;
                Thread.Sleep(80);
            }
        }

        // Orquesta la selección del día de pago como "X de cada mes"
        private void SelectDiaPago(string dia)
        {
            // Abrir select2 del campo día de vencimiento
            OpenSelect2(DiaPagoS2Container);

            // Texto exacto que renderiza Select2 (según tu DOM): "7 de cada mes"
            string visible = $"{dia} de cada mes";

            // Seleccionar
            PickSelect2OptionByVisibleText(visible);
        }

        // ──────────────────────────────────────────────────────────────
        // HELPERS
        // ──────────────────────────────────────────────────────────────
        private IWebElement FirstVisible(By by, int seconds = 2)
        {
            var end = DateTime.UtcNow.AddSeconds(seconds);
            while (DateTime.UtcNow < end)
            {
                try
                {
                    var el = driver.FindElement(by);
                    if (el.Displayed) return el;
                }
                catch { }
                Thread.Sleep(70);
            }
            return null;
        }

        private IWebElement FirstPresent(By by, int seconds = 2)
        {
            var end = DateTime.UtcNow.AddSeconds(seconds);
            while (DateTime.UtcNow < end)
            {
                var list = driver.FindElements(by);
                if (list.Count > 0) return list[0];
                Thread.Sleep(60);
            }
            return null;
        }

        private IWebElement FindSibling(IWebElement from, By siblingLocator)
        {
            try
            {
                // busca en un contenedor padre cercano
                var parent = from.FindElement(By.XPath("./ancestor::div[1]"));
                var s2 = parent.FindElements(siblingLocator);
                if (s2.Count > 0) return s2[0];
            }
            catch { }

            try
            {
                var container = from.FindElement(By.XPath("./ancestor::div[contains(@class,'form-group') or contains(@class,'col') or contains(@class,'row')][1]"));
                var s2 = container.FindElements(siblingLocator);
                if (s2.Count > 0) return s2[0];
            }
            catch { }

            return null;
        }

        private void TrySelectByTextFlexible(SelectElement select, string text)
        {
            // 1) literal
            try { select.SelectByText(text); return; } catch { }

            // 2) normalizado sin acentos/case-insensitive
            string norm(string s)
            {
                if (s == null) return "";
                var formD = s.Normalize(NormalizationForm.FormD);
                var sb = new StringBuilder();
                foreach (var ch in formD)
                {
                    var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                    if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                        sb.Append(ch);
                }
                return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant().Trim();
            }

            string needle = norm(text);
            foreach (var opt in select.Options)
            {
                if (norm(opt.Text).Contains(needle))
                {
                    opt.Click();
                    return;
                }
            }

            // 3) por value
            try { select.SelectByValue(text); return; } catch { }
        }

        private void SafeClick(IWebElement el)
        {
            try
            {
                utilities.ScrollViewElement(el);
                el.Click();
            }
            catch { JsClick(el); }
        }

        private void JsClick(IWebElement el)
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el);
            }
            catch { }
        }

        private void EnsureCheckboxState(IWebElement input, bool desired)
        {
            utilities.ScrollViewElement(input);
            if (input.Selected == desired) return;

            try { input.Click(); } catch { JsClick(input); }
            Thread.Sleep(100);
            if (input.Selected != desired)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript(@"
                    var c = arguments[0];
                    c.checked = arguments[1];
                    c.dispatchEvent(new Event('input',{bubbles:true}));
                    c.dispatchEvent(new Event('change',{bubbles:true}));
                    c.dispatchEvent(new MouseEvent('click', {bubbles:true}));
                ", input, desired);
                Thread.Sleep(80);
            }
        }

        private void SendEvents(IWebElement el)
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript(@"
                    var e = arguments[0];
                    e.dispatchEvent(new Event('input',{bubbles:true}));
                    e.dispatchEvent(new Event('change',{bubbles:true}));
                ", el);
            }
            catch { }
        }

        private IWebElement WaitPresent(By by, int seconds = 2)
        {
            var end = DateTime.UtcNow.AddSeconds(seconds);
            while (DateTime.UtcNow < end)
            {
                var list = driver.FindElements(by);
                if (list.Count > 0) return list[0];
                Thread.Sleep(70);
            }
            return null;
        }

        private void ClickHard(By by, int timeoutSec = 3)
        {
            var el = WaitPresent(by, timeoutSec);
            if (el == null) throw new WebDriverTimeoutException($"Timeout esperando visible: {by}");
            try { el.Click(); } catch { JsClick(el); }
        }

        public void CommitAngularChanges()
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                wait.Until(driver =>
                {
                    try
                    {
                        // Espera que Angular no tenga solicitudes pendientes
                        var js = (IJavaScriptExecutor)driver;
                        bool stable = (bool)js.ExecuteScript(
                            "return (window.angular === undefined) || " +
                            "(angular.element(document.body).injector() === undefined) || " +
                            "(angular.element(document.body).injector().get('$http').pendingRequests.length === 0)");
                        return stable;
                    }
                    catch
                    {
                        // Si no es Angular o no se puede evaluar, continuar
                        return true;
                    }
                });
            }
            catch
            {
                // No bloquear si falla; continuar el flujo
            }
        }
    }
}
