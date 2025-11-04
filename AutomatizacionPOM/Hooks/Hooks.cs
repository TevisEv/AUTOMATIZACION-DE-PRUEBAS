using AutomatizacionPOM.Utility;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll.BoDi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizacionPOM.Hooks
{
    [Binding]
    public sealed  class Hooks :ExtentReport
    {
        private IObjectContainer _container;

        public Hooks(IObjectContainer container)
        {
            _container = container;
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            Console.WriteLine("Running before test run...");
            ExtentReportInit();
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            Console.WriteLine("Running after test run...");
            ExtentReportTearDown();
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            Console.WriteLine("Running before feature...");
            _feature = _extentReports.CreateTest<Feature>(featureContext.FeatureInfo.Title);
        }

        [AfterFeature]
        public static void AfterFeature()
        {
            Console.WriteLine("Running after feature...");
        }


        [BeforeScenario("@Testers")]
        public void BeforeScenarioWithTag()
        {
            Console.WriteLine("Runnig inside tagged hooks in SpecFlow");
        }

        [BeforeScenario(Order = 1)]
        public void FirstBeforeScenario(ScenarioContext scenarioContext)
        {
            IWebDriver driver = new ChromeDriver();
            driver.Manage().Window.Maximize();

            _container.RegisterInstanceAs<IWebDriver>(driver);

            _scenario = _feature.CreateNode<Scenario>(scenarioContext.ScenarioInfo.Title);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            var driver = _container.Resolve<IWebDriver>();

            if (driver != null)
            {
                driver.Quit();
            }
        }

        [AfterStep]
        public void AfterStep(ScenarioContext scenarioContext)
        {
            Console.WriteLine("Running after step...");
            string stepType = scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
            string stepName = scenarioContext.StepContext.StepInfo.Text;
            var driver = _container.Resolve<IWebDriver>();

            // When scenario passed
            if (scenarioContext.TestError == null)
            {
                if (stepType == "Given")
                {
                    _scenario.CreateNode<Given>(stepName);
                }
                else if (stepType == "When")
                {
                    _scenario.CreateNode<When>(stepName);
                }
                else if (stepType == "Then")
                {
                    _scenario.CreateNode<Then>(stepName);
                }
            }

            //When scenario fails
            if (scenarioContext.TestError != null)
            {
                var screenshotPath = addScreenshot(driver, scenarioContext); 
                var media = MediaEntityBuilder
                    .CreateScreenCaptureFromPath(screenshotPath)
                    .Build();

                switch (stepType)
                {
                    case "Given":
                        _scenario.CreateNode<Given>(stepName).Fail(scenarioContext.TestError.Message, media);
                        break;
                    case "When":
                        _scenario.CreateNode<When>(stepName).Fail(scenarioContext.TestError.Message, media);
                        break;
                    case "Then":
                        _scenario.CreateNode<Then>(stepName).Fail(scenarioContext.TestError.Message, media);
                        break;
                    case "And":
                        _scenario.CreateNode<And>(stepName).Fail(scenarioContext.TestError.Message, media);
                        break;
                }
            }
        }
    }
}
