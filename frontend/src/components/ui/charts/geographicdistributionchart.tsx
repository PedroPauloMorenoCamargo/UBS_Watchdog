import type { TransactionCountry } from "@/mocks/mocks";

interface GeoDistributionChartProps {
  data: TransactionCountry[];
}

export function GeographicDistributionChart({
  data,
}: GeoDistributionChartProps) {
  const totalAmount = data.reduce(
    (sum, item) => sum + item.totalAmount,
    0
  );
  const formatAmount = (value: number): string => {
  if (value >= 1_000_000) {
    return `${(value / 1_000_000).toFixed(1)}M`; // 380000000 -> 380.0M
  }
  return value.toString(); // valores menores ficam normais
};

  return (
    <div className="space-y-5">
      {data.map((item) => {
        const percentage =
          totalAmount === 0
            ? 0
            : (item.totalAmount / totalAmount) * 100;

        return (
          <div key={item.country}>
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-gray-900">
                {item.country}
              </span>

              <span className="text-sm text-gray-900">
                ${formatAmount(item.totalAmount)}
                <span className="ml-2 text-xs text-gray-500">
                  ({item.count} txns)
                </span>
              </span>
            </div>
            <div className="w-full h-2 rounded-full bg-gray-200 overflow-hidden">
              <div
                className="h-full bg-[#e60028] rounded-full transition-all"
                style={{ width: `${percentage}%` }}
              />
            </div>
          </div>
        );
      })}
    </div>
  );
}
