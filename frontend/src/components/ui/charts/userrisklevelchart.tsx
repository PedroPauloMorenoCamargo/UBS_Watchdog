import {
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
  Tooltip,
} from "recharts";

import { RISK_COLORS } from "@/constants/colors";

type RiskLevelData = {
  name: string;
  value: number;
};

interface UsersByRiskLevelChartProps {
  data: RiskLevelData[];
}

export function UsersByRiskLevelChart({
  data,
}: UsersByRiskLevelChartProps) {
  const filteredData = data.filter(d => d.value > 0);

  return (
    <div className="flex h-64 w-full flex-col">
      <ResponsiveContainer width="100%" height="100%">
        <PieChart>
          <Pie
            data={filteredData}
            dataKey="value"
            nameKey="name"
            outerRadius={95}
          >
             {filteredData.map(item => (
              <Cell
                key={`cell-${item.name}`}
                fill={RISK_COLORS[item.name]}
              />
            ))}
          </Pie>

          <Tooltip />
        </PieChart>
      </ResponsiveContainer>
      <div className="mb-4 flex justify-center gap-4 text-sm">
        {filteredData.map(item => (
          <div key={item.name} className="flex items-center gap-2">
            <span
              className="h-3 w-3 rounded"
              style={{ backgroundColor: RISK_COLORS[item.name] }}
            />
            <span className="text-slate-600">{item.name}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
