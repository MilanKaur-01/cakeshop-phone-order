using Azure.Communication.Sms;
using cakeShopMinimalApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace cakeShopMinimalApi
{
    public class CakeOrderPlugin
    {
        private CakeDb cakeDb;
        private SmsClient smsClient;
        private string cakeShopPhoneNumber;

        public CakeOrderPlugin(string acsConnectionString, string cakeShopPhoneNumber)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CakeDb>();
            optionsBuilder.UseInMemoryDatabase("items");

            this.cakeDb = new CakeDb(optionsBuilder.Options);
            this.smsClient = new SmsClient(acsConnectionString);
            this.cakeShopPhoneNumber = cakeShopPhoneNumber;
        }

        [KernelFunction, Description("Add cake order to system and send an SMS confirmation")]
        public async Task AddCakeOrder(
            [Description("The cake flavour, e.g. Velvet Indulgence, Choco Lover's Dream")] CakeFlavour? cakeFlavour = null,
            [Description("The cake size, e.g. 6 inches, 8 inches")] CakeSize? cakeSize = null,
            [Description("The cake price, e.g. 35, 65")] int? cakePrice = null,
            [Description("Customer phone number, e.g. +11234567890")] string? customerPhoneNumber = null
        )
        {
            var cake = new Cake()
            {
                CakeFalvour = cakeFlavour,
                CakeSize = cakeSize,
                CakePrice = cakePrice
            };

            await cakeDb.Cakes.AddAsync(cake);
            await cakeDb.SaveChangesAsync();

            if (customerPhoneNumber != null)
            {
                var orderMessage = "Thank you for ordering " + cake.CakeSize.ToString() + " inches " + cake.CakeFalvour.ToString() + " cake from Milan CakeShop. Here's you order reference#: " + cake.Id.ToString();
                await SendOrderConfirmation(customerPhoneNumber, orderMessage);
            }
        }

        [KernelFunction, Description("Gets cake order information from the system")]
        public async Task<Cake?> GetCakeOrder(
            [Description("The cake order reference Number, e.g. 1234")] int cakeId
        )
        {   
            return await cakeDb.Cakes.FindAsync(cakeId);
        }

        [KernelFunction, Description("Update cake order information in the system and send an SMS confirmation")]
        public async Task UpdateCakeOrder(
            [Description("The cake order reference Number, e.g. 1234")] int cakeId,
            [Description("The cake flavour, e.g. Velvet Indulgence, Choco Lover's Dream")] CakeFlavour? cakeFlavour = null,
            [Description("The cake size, e.g. 6 inches, 8 inches")] CakeSize? cakeSize = null,
            [Description("The cake price, e.g. 35, 65")] int? cakePrice = null,
            [Description("Customer phone number, e.g. +11234567890")] string? customerPhoneNumber = null
        )
        {
            var updateCake = new Cake()
            {
                CakeFalvour = cakeFlavour,
                CakeSize = cakeSize,
                CakePrice = cakePrice
            };

            var cake = await cakeDb.Cakes.FindAsync(cakeId);
            if (cake != null)
            {
                cake.CakeFalvour = updateCake.CakeFalvour;
                cake.CakeSize = updateCake.CakeSize;
                cake.CakePrice = updateCake.CakePrice;
                await cakeDb.SaveChangesAsync();

                if (customerPhoneNumber != null)
                {
                    var orderMessage = "Thank you for ordering " + cake.CakeSize.ToString() + " inches " + cake.CakeFalvour.ToString() + " cake from Milan CakeShop. Here's you order reference#: " + cake.Id.ToString();
                    await SendOrderConfirmation(customerPhoneNumber, orderMessage);
                }
            }
        }

        [KernelFunction, Description("Cancels cake order from system and send an SMS confirmation")]
        public async Task CancelCakeOrder(
            [Description("The cake order reference Number, e.g. 1234")] int cakeId,
            [Description("Customer phone number, e.g. +11234567890")] string? customerPhoneNumber = null
        )
        {
            var cake = await cakeDb.Cakes.FindAsync(cakeId);
            if (cake != null)
            {
                cakeDb.Cakes.Remove(cake);
                await cakeDb.SaveChangesAsync();

                if (customerPhoneNumber != null)
                {
                    var orderMessage = "Your order for " + cake.CakeSize.ToString() + " inches " + cake.CakeFalvour.ToString() + " cake from Milan CakeShop has been cancelled. Here's you order reference#: " + cake.Id.ToString();
                    await SendOrderConfirmation(customerPhoneNumber, orderMessage);
                }
            }
        }

        private async Task SendOrderConfirmation(string customerPhoneNumber, string orderMessage)
        {
            await smsClient.SendAsync(
                from: cakeShopPhoneNumber,
                to: customerPhoneNumber,
                message: orderMessage
    );
        }
    }
}
