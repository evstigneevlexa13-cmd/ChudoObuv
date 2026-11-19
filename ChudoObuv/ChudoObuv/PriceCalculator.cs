using System;

namespace ChudoObuv
{
    public static class PriceCalculator
    {
        private const decimal DiscountPercent = 25m;

        public static void GetPreviousMonthRange(
            DateTime calculationDate,
            out DateTime startDate,
            out DateTime endDate)
        {
            endDate = new DateTime(
                calculationDate.Year,
                calculationDate.Month,
                1);

            startDate = endDate.AddMonths(-1);
        }

        public static decimal GetFinalPrice(
            decimal basePrice,
            bool hadOrdersLastMonth)
        {
            if (hadOrdersLastMonth)
            {
                return basePrice;
            }

            decimal discount = basePrice * DiscountPercent / 100m;
            return Math.Round(basePrice - discount, 2);
        }
    }
}