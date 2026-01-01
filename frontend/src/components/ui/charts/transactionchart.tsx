import {
  PieChart,
  Pie,
  Cell,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from "recharts";
import { COLORS, transactionsByType } from "@/mocks/mocks";

export function TransactionsByTypeChart() {
  return (
    <div className="h-64">

    <ResponsiveContainer width="100%" height="100%">
      <PieChart>
        <Pie
          data={transactionsByType}
          dataKey="value"
          nameKey="name"
          innerRadius={60}
          outerRadius={90}
          paddingAngle={3}
        >
          {transactionsByType.map((_, index) => (
            <Cell key={index} fill={COLORS[index]} />
          ))}
        </Pie>

        <Tooltip />
        <Legend verticalAlign="bottom" />
      </PieChart>
    </ResponsiveContainer>
    </div>
  );
}
