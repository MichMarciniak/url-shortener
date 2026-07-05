using System.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace UrlShortener.Tests.e2e;

public class SeleniumTests : IDisposable
{
    private readonly IWebDriver _driver;

    private const string BaseUrl = "http://localhost:5140";


    public SeleniumTests()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");

        _driver = new ChromeDriver(options);
        _driver.Manage().Window.Maximize();
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    [Fact]
    public void ShortenUrl_ShouldGenerateShortenedLink()
    {
        _driver.Navigate().GoToUrl(BaseUrl);

        var urlInput = _driver.FindElement(By.Name("url"));
        var submitButton = _driver.FindElement(By.CssSelector("button[type='submit']"));

        urlInput.Clear();
        urlInput.SendKeys("https://google.com");
        
        submitButton.Click();
        
        var resultParagraph = _driver.FindElement(By.ClassName("result"));

        Assert.Contains(BaseUrl, resultParagraph.Text);
    }

    [Fact]
    public void ShortenUrl_ShouldGenerateCustomLink()
    {
        string customAlias = "test-alias-" + Guid.NewGuid().ToString().Substring(0, 5);
 
        _driver.Navigate().GoToUrl(BaseUrl);
        
        var urlInput = _driver.FindElement(By.Name("url"));
        var customInput = _driver.FindElement(By.Name("custom"));
        var submitButton = _driver.FindElement(By.CssSelector("button[type=submit]"));
        
        urlInput.Clear();
        urlInput.SendKeys("https://google.com");
        
        customInput.Clear();
        customInput.SendKeys(customAlias);
        
        submitButton.Click();
        
        var resultParagraph = _driver.FindElement(By.ClassName("result"));
        
        var expectedUrl = $"{BaseUrl.TrimEnd('/')}/{customAlias}";
        Assert.Contains(expectedUrl, resultParagraph.Text);
    }

    [Fact]
    public void ShortenUrl_ShouldReturnErrorWhenAliasIsTaken()
    {
        _driver.Navigate().GoToUrl(BaseUrl);
        var urlInput = _driver.FindElement(By.Name("url"));
        var customInput = _driver.FindElement(By.Name("custom"));
        var submitButton = _driver.FindElement(By.CssSelector("button[type=submit]"));
        
        urlInput.Clear();
        urlInput.SendKeys("https://google.com");
        
        customInput.Clear();
        customInput.SendKeys("test");
        
        submitButton.Click();
        
        submitButton.Click();
        
        var errorParagraph = _driver.FindElement(By.ClassName("error"));
        
        Assert.Contains("Error: Short form already exists.", errorParagraph.Text);
    }

    [Fact]
    public void ShortenUrl_ShouldReturnErrorWhenUrlIsEmpty()
    {
        
        _driver.Navigate().GoToUrl(BaseUrl);
        var urlInput = _driver.FindElement(By.Name("url"));
        var customInput = _driver.FindElement(By.Name("custom"));
        var submitButton = _driver.FindElement(By.CssSelector("button[type=submit]"));
        
        urlInput.Clear();
        
        submitButton.Click();
        
        var errorParagraph = _driver.FindElement(By.ClassName("error"));

        Assert.Contains("Error: Url is empty", errorParagraph.Text);
    }
    
    
    
    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}