namespace cakeShopMinimalApi.Model
{
    public class Cake
    {
        public int Id { get; set; }

        public CakeFlavour? CakeFalvour { get; set; }

        public CakeSize? CakeSize { get; set; }

        public int? CakePrice { get; set; }
    }
}
