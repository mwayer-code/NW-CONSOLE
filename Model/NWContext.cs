using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;


namespace Northwind_Console.Model
{
    public partial class NWContext : DbContext
    {
        public NWContext()
        {
        }

        public NWContext(DbContextOptions<NWContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<Shipper> Shippers { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<Territory> Territories { get; set; }

        public void EditCategory(Category UpdatedCategory)
        {
            Category category = this.Categories.Find(UpdatedCategory.CategoryId);
            category.CategoryName = UpdatedCategory.CategoryName;
            this.SaveChanges();
        }
        
        public void DeleteCategory(int categoryId)
        {
            Category category = this.Categories.Include(c => c.Products).FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Category not found.");
                return;
            }
            if (category.Products.Any())
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("This category has related products. Please enter the ID of the category you want to reassign the products to, or enter 'delete' to delete the products:");

                string input = Console.ReadLine();
                if (input.ToLower() != "delete")
                {
                    int newCategoryId;
                    if (int.TryParse(input, out newCategoryId))
                    {
                        Category newCategory = this.Categories.FirstOrDefault(c => c.CategoryId == newCategoryId);
                        if (newCategory != null)
                        {
                            foreach (Product product in category.Products)
                            {
                                product.Category = newCategory;
                            }
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Products reassigned to new category.");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Category not found. No changes were made.");
                            return;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid input. No changes were made.");
                        return;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("You have chosen to delete the products. Are you sure? (yes/no)");
                    string confirmation = Console.ReadLine();
                    if (confirmation.ToLower() == "yes")
                    {
                        this.Products.RemoveRange(category.Products);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Products deleted.");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No changes were made.");
                        return;
                    }
                }
            }
            this.Categories.Remove(category);
            this.SaveChanges();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Category deleted successfully.");
            Console.ResetColor();
        }

        public void DisplayCategories()
        {   
            Console.ForegroundColor = ConsoleColor.Green;
            var categories = this.Categories.ToList();

            if (!categories.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No categories found.");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Categories:");
            foreach (var category in categories)
            {
                Console.WriteLine($"ID: {category.CategoryId}, Name: {category.CategoryName}, Description: {category.Description}");
            }
            Console.ResetColor();
        }

        public bool AddProduct(string productName, string quantityPerUnit, decimal unitPrice, short unitsInStock, short unitsOnOrder, short reorderLevel, bool discontinued, int categoryId)
        {
            if (this.Products.Any(p => p.ProductName.ToLower() == productName.ToLower()))
            {
                return false;

            }

            Product product = new Product
            {
                ProductName = productName,
                QuantityPerUnit = quantityPerUnit,
                UnitPrice = unitPrice,
                UnitsInStock = unitsInStock,
                UnitsOnOrder = unitsOnOrder,
                ReorderLevel = reorderLevel,
                Discontinued = discontinued,
                CategoryId = categoryId
            };

            this.Products.Add(product);
            this.SaveChanges();

            return true;
        }

        public void EditProduct(Product UpdatedProduct)
        {

            Product product = this.Products.Find(UpdatedProduct.ProductId);

            if (product != null)
            {
                product.ProductName = UpdatedProduct.ProductName;
                product.QuantityPerUnit = UpdatedProduct.QuantityPerUnit;
                product.UnitPrice = UpdatedProduct.UnitPrice;
                product.UnitsInStock = UpdatedProduct.UnitsInStock;
                product.UnitsOnOrder = UpdatedProduct.UnitsOnOrder;
                product.ReorderLevel = UpdatedProduct.ReorderLevel;
                product.Discontinued = UpdatedProduct.Discontinued;
                product.CategoryId = UpdatedProduct.CategoryId;
            }
            
            this.SaveChanges();
        }

        public void DeleteProduct(int productId)
        {
            Product product = this.Products.Find(productId);
            if (product == null)
            {
                Console.WriteLine("Product not found.");
                return;
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Are you sure you want to delete {product.ProductName}? (yes/no)");
            string confirmation = Console.ReadLine();

            if (confirmation.ToLower() == "yes")
            {
                this.Products.Remove(product);
                this.SaveChanges();  
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{product.ProductName} deleted successfully.");
            }
            else{
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("No changes were made.");
            }

            Console.ResetColor();
        }

        public void DisplayProductsByCategory(){
            var categories = this.Categories.Include(c => c.Products).ToList();

            if (!categories.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No categories found.");
                return;
            }

            foreach (var category in categories)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Category: {category.CategoryName}");
                foreach (var product in category.Products)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"ID: {product.ProductId}, Name: {product.ProductName}, Active:  {(!product.Discontinued ? "Yes" : "No")}");
                }
                Console.ResetColor();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                 IConfiguration config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
                optionsBuilder.UseSqlServer(@config["Northwind:ConnectionString"]);

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(15);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(e => e.Address).HasMaxLength(60);

                entity.Property(e => e.City).HasMaxLength(25);

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.Country).HasMaxLength(15);

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.Fax).HasMaxLength(24);

                entity.Property(e => e.Phone).HasMaxLength(24);

                entity.Property(e => e.PostalCode).HasMaxLength(10);

                entity.Property(e => e.Region).HasMaxLength(15);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Address).HasMaxLength(60);

                entity.Property(e => e.BirthDate).HasColumnType("datetime");

                entity.Property(e => e.City).HasMaxLength(15);

                entity.Property(e => e.Country).HasMaxLength(15);

                entity.Property(e => e.Extension).HasMaxLength(4);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.HireDate).HasColumnType("datetime");

                entity.Property(e => e.HomePhone).HasMaxLength(24);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.PostalCode).HasMaxLength(10);

                entity.Property(e => e.Region).HasMaxLength(15);

                entity.Property(e => e.Title).HasMaxLength(30);

                entity.Property(e => e.TitleOfCourtesy).HasMaxLength(25);

                entity.HasOne(d => d.ReportsToNavigation)
                    .WithMany(p => p.InverseReportsToNavigation)
                    .HasForeignKey(d => d.ReportsTo)
                    .HasConstraintName("FK_Employees_Employees");

                entity.HasMany(d => d.Territories)
                    .WithMany(p => p.Employees)
                    .UsingEntity<Dictionary<string, object>>(
                        "EmployeeTerritory",
                        l => l.HasOne<Territory>().WithMany().HasForeignKey("TerritoryId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_EmployeeTerritories_Territories"),
                        r => r.HasOne<Employee>().WithMany().HasForeignKey("EmployeeId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_EmployeeTerritories_Employees"),
                        j =>
                        {
                            j.HasKey("EmployeeId", "TerritoryId").IsClustered(false);

                            j.ToTable("EmployeeTerritories");

                            j.IndexerProperty<string>("TerritoryId").HasMaxLength(20);
                        });
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.Freight)
                    .HasColumnType("money")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.OrderDate).HasColumnType("datetime");

                entity.Property(e => e.RequiredDate).HasColumnType("datetime");

                entity.Property(e => e.ShipAddress).HasMaxLength(60);

                entity.Property(e => e.ShipCity).HasMaxLength(15);

                entity.Property(e => e.ShipCountry).HasMaxLength(15);

                entity.Property(e => e.ShipName).HasMaxLength(40);

                entity.Property(e => e.ShipPostalCode).HasMaxLength(10);

                entity.Property(e => e.ShipRegion).HasMaxLength(15);

                entity.Property(e => e.ShippedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_Orders_Customers");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK_Orders_Employees");

                entity.HasOne(d => d.ShipViaNavigation)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.ShipVia)
                    .HasConstraintName("FK_Orders_Shippers");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.OrderDetailsId);

                entity.Property(e => e.Discount).HasColumnType("decimal(5, 3)");

                entity.Property(e => e.Quantity).HasDefaultValueSql("((1))");

                entity.Property(e => e.UnitPrice).HasColumnType("money");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderDetails_Orders");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderDetails_Products");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.QuantityPerUnit).HasMaxLength(20);

                entity.Property(e => e.ReorderLevel).HasDefaultValueSql("((0))");

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("money")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.UnitsInStock).HasDefaultValueSql("((0))");

                entity.Property(e => e.UnitsOnOrder).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK_Products_Categories");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.SupplierId)
                    .HasConstraintName("FK_Products_Suppliers");
            });

            modelBuilder.Entity<Region>(entity =>
            {
                entity.HasKey(e => e.RegionId)
                    .IsClustered(false);

                entity.ToTable("Region");

                entity.Property(e => e.RegionId).ValueGeneratedNever();

                entity.Property(e => e.RegionDescription)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Shipper>(entity =>
            {
                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.Phone).HasMaxLength(24);
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.Property(e => e.Address).HasMaxLength(60);

                entity.Property(e => e.City).HasMaxLength(15);

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.ContactName).HasMaxLength(30);

                entity.Property(e => e.ContactTitle).HasMaxLength(30);

                entity.Property(e => e.Country).HasMaxLength(15);

                entity.Property(e => e.Fax).HasMaxLength(24);

                entity.Property(e => e.Phone).HasMaxLength(24);

                entity.Property(e => e.PostalCode).HasMaxLength(10);

                entity.Property(e => e.Region).HasMaxLength(15);
            });

            modelBuilder.Entity<Territory>(entity =>
            {
                entity.HasKey(e => e.TerritoryId)
                    .IsClustered(false);

                entity.Property(e => e.TerritoryId).HasMaxLength(20);

                entity.Property(e => e.TerritoryDescription)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.Territories)
                    .HasForeignKey(d => d.RegionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Territories_Region");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
