import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  Legend,
  CartesianGrid,
} from "recharts";

type AlertSeverityBarChartProps = {
  data: {
    day: string;
    low: number;
    medium: number;
    high: number;
    critical: number;
  }[];
};

export function AlertSeverityBarChart({ data }: AlertSeverityBarChartProps) {
  return (
    <ResponsiveContainer width="100%" height={300}>
      <BarChart data={data} margin={{ top: 10, right: 20, left: 0, bottom: 5 }}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="day" />
        <YAxis />
        <Tooltip />
        <Legend />
        <Bar dataKey="low" fill="#16a34a" />
        <Bar dataKey="medium" fill="#f59e0b" />
        <Bar dataKey="high" fill="#dc2626" />
        <Bar dataKey="critical" fill="#7f1d1d" />
      </BarChart>
    </ResponsiveContainer>
  );
}
