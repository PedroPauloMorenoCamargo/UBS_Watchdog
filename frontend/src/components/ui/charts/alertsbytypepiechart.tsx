import {
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
  Tooltip,
} from "recharts";
import type { PieLabelRenderProps } from "recharts";

type AlertTypeData = {
  name: string;
  value: number;
};

interface AlertsByTypePieChartProps {
  data: AlertTypeData[];
  height?: number;
}

const ALERT_TYPE_COLORS: Record<string, string> = {
  "Critical": "#ef4444",
  "High": "#f97316",
  "Medium": "#eab308", 
  "Low": "#22c55e", 
};


const SEVERITY_LEVELS = [
  { name: "Critical", color: "#ef4444", bgColor: "bg-red-100", iconColor: "text-red-500" },
  { name: "High", color: "#f97316", bgColor: "bg-orange-100", iconColor: "text-orange-500" },
  { name: "Medium", color: "#eab308", bgColor: "bg-yellow-100", iconColor: "text-yellow-500" },
  { name: "Low", color: "#22c55e", bgColor: "bg-green-100", iconColor: "text-green-500" },
];

const DEFAULT_COLORS = ["#ef4444", "#f97316", "#eab308", "#22c55e"];

export function AlertsByTypePieChart({
  data,
  height = 280,
}: AlertsByTypePieChartProps) {
  const filteredData = data.filter((d) => d.value > 0);

  const renderCustomLabel = (props: PieLabelRenderProps) => {
    const { cx, cy, midAngle, outerRadius, name, value, index } = props;
    
    if (
      typeof cx !== "number" ||
      typeof cy !== "number" ||
      typeof midAngle !== "number" ||
      typeof outerRadius !== "number"
    ) {
      return null;
    }

    const RADIAN = Math.PI / 180;
    const radius = outerRadius + 30;
    const x = cx + radius * Math.cos(-midAngle * RADIAN);
    const y = cy + radius * Math.sin(-midAngle * RADIAN);

    const labelName = String(name ?? "");
    const labelValue = Number(value ?? 0);
    const labelIndex = Number(index ?? 0);
    const color = ALERT_TYPE_COLORS[labelName] || DEFAULT_COLORS[labelIndex % DEFAULT_COLORS.length];

    return (
      <text
        x={x}
        y={y}
        fill={color}
        textAnchor={x > cx ? "start" : "end"}
        dominantBaseline="central"
        fontSize={12}
        fontWeight={500}
      >
        {`${labelName}: ${labelValue}`}
      </text>
    );
  };

  if (filteredData.length === 0) {
    return (
      <div className="flex flex-col" style={{ height }}>
        <div className="flex justify-center gap-6 mb-4">
          {SEVERITY_LEVELS.map((level) => (
            <div key={level.name} className="flex items-center gap-2">
              <span
                className="h-3 w-3 rounded-full"
                style={{ backgroundColor: level.color }}
              />
              <span className="text-sm text-slate-600">{level.name}</span>
            </div>
          ))}
        </div>
        <div className="flex items-center justify-center flex-1">
          <span className="text-slate-500">No alerts for this client</span>
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col" style={{ height }}>
      <div className="flex justify-center gap-6 mb-4">
        {SEVERITY_LEVELS.map((level) => (
          <div key={level.name} className="flex items-center gap-2">
            <span
              className="h-3 w-3 rounded-full"
              style={{ backgroundColor: level.color }}
            />
            <span className="text-sm text-slate-600">{level.name}</span>
          </div>
        ))}
      </div>
      <ResponsiveContainer width="100%" height="100%">
        <PieChart>
          <Pie
            data={filteredData}
            dataKey="value"
            nameKey="name"
            cx="50%"
            cy="50%"
            outerRadius={80}
            label={renderCustomLabel}
            labelLine={false}
          >
            {filteredData.map((item, index) => (
              <Cell
                key={`cell-${item.name}`}
                fill={ALERT_TYPE_COLORS[item.name] || DEFAULT_COLORS[index % DEFAULT_COLORS.length]}
              />
            ))}
          </Pie>
          <Tooltip />
        </PieChart>
      </ResponsiveContainer>
    </div>
  );
}
