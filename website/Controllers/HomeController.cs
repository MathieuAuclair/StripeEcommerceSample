using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using website.Models;

namespace website.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index(int pageSize = 10, int pageNumber = 0, string? last = null)
    {
        var service = new ProductService();
        var priceService = new PriceService();
        var products = service.List(new ProductListOptions { Limit = pageSize, StartingAfter = last })
            .Where(product => product.Active)
            .Select(product =>
            {
                if (!string.IsNullOrWhiteSpace(product?.DefaultPriceId))
                {
                    product.DefaultPrice = priceService.Get(product.DefaultPriceId);
                }

                return product;
            })
            .ToList();

        ViewData["pageSize"] = pageSize;
        ViewData["pageNumber"] = pageNumber;

        if (products.Any())
        {
            ViewData["last"] = products.Last()?.Id;
        }

        return View(products);
    }

    public IActionResult Purchase(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            return BadRequest();
        }

        var productService = new ProductService();
        var product = productService.Get(productId);

        if (product == null || string.IsNullOrWhiteSpace(product.DefaultPriceId))
        {
            return BadRequest();
        }

        var priceService = new PriceService();
        var price = priceService.Get(product.DefaultPriceId);

        if (price == null)
        {
            return NotFound($"The price {product.DefaultPriceId} could not be found for product {product.Id}...");
        }

        product.DefaultPrice = price;

        var service = new PaymentIntentService();
        var paymentIntent = service.Create(new PaymentIntentCreateOptions
        {
            Amount = price.UnitAmount,
            Currency = price.Currency,
            PaymentMethodTypes = new List<string>
            {
                "card",
                "cashapp",
                "ach_credit_transfer"
            },
            Metadata = new Dictionary<string, string>
            {
                { "product_id", productId }
            },
        });

        ViewData["product"] = product;

        return View(paymentIntent);
    }

    public IActionResult Confirm()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
