using NLog;
using System.Linq;
using Northwind_Console.Model;
// using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + "\\nlog.config";

// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
logger.Info("Program started");

try
{
    var db = new NWContext();
    string choice;
    Console.ForegroundColor = ConsoleColor.White;

    do
    {
        Console.WriteLine("1) Display Categories");
        Console.WriteLine("2) Add Category");
        Console.WriteLine("3) Edit Category");
        Console.WriteLine("4) Delete Category");
        Console.WriteLine("5) Display Category and related products");
        Console.WriteLine("6) Display all Categories and their related products");
        Console.WriteLine("7) Display Products");
        Console.WriteLine("8) Search for a Product");
        Console.WriteLine("9) Add Product");
        Console.WriteLine("10) Edit Product");
        Console.WriteLine("11) Delete Product");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("\"q\" to quit");
        Console.ForegroundColor = ConsoleColor.White;
        choice = Console.ReadLine();
        Console.Clear();
        logger.Info($"Option {choice} selected");

        Console.ResetColor();

        switch (choice)
        {
            case "1":
                var query1 = db.Categories.OrderBy(p => p.CategoryName);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{query1.Count()} records returned");
                Console.ForegroundColor = ConsoleColor.Magenta;
                foreach (var item in query1)
                {
                    Console.WriteLine($"{item.CategoryName} - {item.Description}");
                }
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case "2":
                Category category = new Category();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Enter Category Name:");
                category.CategoryName = Console.ReadLine();
                Console.WriteLine("Enter the Category Description:");
                category.Description = Console.ReadLine();

                ValidationContext context = new ValidationContext(category, null, null);
                List<ValidationResult> results = new List<ValidationResult>();

                var isValid = Validator.TryValidateObject(category, context, results, true);
                if (isValid)
                {
                    logger.Info("Validation passed");
                    // check for unique name
                    if (!db.IsCategoryNameUnique(category.CategoryName))
                    {
                        // generate validation error
                        isValid = false;
                        results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                    }
                    else
                    {
                        logger.Info("Validation passed");
                        db.AddCategory(category);
                        logger.Info("Category added - {category.CategoryName}");
                    }
                }
                if (!isValid)
                {
                    foreach (var result in results)
                    {
                        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                    }
                }
                Console.ResetColor();
                break;
            case "3":
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Select the category you want to edit:");
                var query2 = db.Categories.OrderBy(p => p.CategoryId);
                Console.ForegroundColor = ConsoleColor.Cyan;
                foreach (var item in query2)
                {
                    Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                }
                Console.ForegroundColor = ConsoleColor.White;
                int id1 = int.Parse(Console.ReadLine());
                Category category1 = db.Categories.FirstOrDefault(c => c.CategoryId == id1);

                string oldName = category1.CategoryName;

                Console.WriteLine("Enter the new Category Name:");
                category1.CategoryName = Console.ReadLine();
                Console.WriteLine("Enter the new Category Description:");
                category1.Description = Console.ReadLine();

                db.EditCategory(category1);
                logger.Info("Category updated - from {oldName} to {name}", oldName, category1.CategoryName);

                break;
            case "4":
                db.DisplayCategories();

                Console.WriteLine("Select the ID you want to delete:");
                int deleteId = int.Parse(Console.ReadLine());
                db.DeleteCategory(deleteId);
                break;
            case "5":
                var query3 = db.Categories.OrderBy(p => p.CategoryId);

                Console.WriteLine("Select the category whose products you want to display:");
                Console.ForegroundColor = ConsoleColor.Cyan;
                foreach (var item in query3)
                {
                    Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                }
                Console.ForegroundColor = ConsoleColor.White;
                int id2 = int.Parse(Console.ReadLine());
                Console.Clear();
                logger.Info($"CategoryId {id2} selected");
                Category category2 = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id2);
                Console.WriteLine($"{category2.CategoryName} - {category2.Description}");
                foreach (Product p in category2.Products)
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
                break;
            case "6":
                db.DisplayProductsByCategory();
                break;
            case "7":
                Console.WriteLine("Which products would you like to see? (all/discontinued/active)");
                string productType = Console.ReadLine().ToLower();

                IQueryable<Product> products;

                switch (productType)
                {
                    case "discontinued":
                        products = db.Products.Where(p => p.Discontinued).OrderBy(p => p.ProductName);
                        break;
                    case "active":
                        products = db.Products.Where(p => !p.Discontinued).OrderBy(p => p.ProductName);
                        break;
                    case "all":
                    default:
                        products = db.Products.OrderBy(p => p.ProductName);
                        break;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{products.Count()} records returned");
                Console.ForegroundColor = ConsoleColor.Magenta;
                foreach (var item in products)
                {
                    Console.WriteLine($"{item.ProductName} - {(item.Discontinued ? "Discontinued" : "Active")}");
                }
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case "8":
                Console.WriteLine("Enter the product name:");
                string productName1 = Console.ReadLine();

                var product = db.Products.Where(p => p.ProductName.Contains(productName1)).OrderBy(p => p.ProductName);

                if (product == null || !product.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No records found");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{product.Count()} records returned");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    foreach (var item in product)
                    {
                        Console.WriteLine($"{item.ProductName} - {(item.Discontinued ? "Discontinued" : "Active")}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
                break;
            case "9":
                Console.WriteLine("Enter Product Name:");
                string productName2 = Console.ReadLine();

                if (db.Products.Any(p => p.ProductName.ToLower() == productName2.ToLower()))
                {
                    logger.Error("Product {0} already exists! Please try again with a different name", productName2);
                }

                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Enter the Product Quantity Per Unit:");
                    string quantityPerUnit = Console.ReadLine();
                    Console.WriteLine("Enter the Product Unit Price:");
                    decimal unitPrice = decimal.Parse(Console.ReadLine());
                    Console.WriteLine("Enter the Product Units in Stock:");
                    short unitsInStock = short.Parse(Console.ReadLine());
                    Console.WriteLine("Enter the Product Units on Order:");
                    short unitsOnOrder = short.Parse(Console.ReadLine());
                    Console.WriteLine("Enter the Product Reorder Level:");
                    short reorderLevel = short.Parse(Console.ReadLine());
                    Console.WriteLine("Is the Product Discontinued? (y/n)");
                    bool discontinued = Console.ReadLine().ToLower() == "y";
                
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Select the Category for the Product:");
                    var query4 = db.Categories.OrderBy(p => p.CategoryId);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    foreach (var item in query4)
                    {
                        Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    int categoryId = int.Parse(Console.ReadLine());

                    bool productAdded = db.AddProduct(productName2, quantityPerUnit, unitPrice, unitsInStock, unitsOnOrder, reorderLevel, discontinued, categoryId);
                    if (productAdded)
                    {
                        logger.Info("Product added - {ProductName}", productName2);
                    }
                }
                Console.ResetColor();
                break;
            case "10":
                Console.WriteLine("Enter the product name:");
                string productName = Console.ReadLine();

                var product2 = db.Products.FirstOrDefault(p => p.ProductName == productName);

                if (product2 == null)
                {
                    logger.Error("Product {0} not found", productName);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Enter the Product Quantity Per Unit:");
                    string quantityPerUnit = Console.ReadLine();
                    Console.WriteLine("Enter the Product Unit Price:");
                    decimal unitPrice = decimal.Parse(Console.ReadLine());
                    Console.WriteLine("Enter the Product Units in Stock:");
                    short unitsInStock = short.Parse(Console.ReadLine());
                    Console.WriteLine("Enter the Product Units on Order:");
                    short unitsOnOrder = short.Parse(Console.ReadLine());
                    Console.WriteLine("Enter the Product Reorder Level:");
                    short reorderLevel = short.Parse(Console.ReadLine());
                    Console.WriteLine("Is the Product Discontinued? (y/n)");
                    bool discontinued = Console.ReadLine().ToLower() == "y";

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Select the Category for the Product:");
                    var query = db.Categories.OrderBy(p => p.CategoryId);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    foreach (var item in query)
                    {
                        Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    int categoryId = int.Parse(Console.ReadLine());

                    Product updatedProduct = new ()
                    {
                        ProductId = product2.ProductId,
                        ProductName = productName,
                        QuantityPerUnit = quantityPerUnit,
                        UnitPrice = unitPrice,
                        UnitsInStock = unitsInStock,
                        UnitsOnOrder = unitsOnOrder,
                        ReorderLevel = reorderLevel,
                        Discontinued = discontinued,
                        CategoryId = categoryId
                    };
                    db.EditProduct(updatedProduct);
                    logger.Info("Product updated - {ProductName}", productName);
                }
                break;
            case "11":
                db.DisplayProductsByCategory();
                Console.WriteLine("Enter the ID of the product you want to delete:");
                int id3 = int.Parse(Console.ReadLine());
                db.DeleteProduct(id3);
                break;
            case "q":
                logger.Info("Program ended");
                break;
            default:
                logger.Info("Invalid selection");
                break;
        }
        Console.WriteLine();
    } while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}
finally
{
    LogManager.Shutdown();
}