import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import { alertsBySeverity } from "@/mocks/mocks";

export function AlertsBySeverityChart() {
  return (
    <ResponsiveContainer width="100%" height="100%">
      <BarChart data={alertsBySeverity}>
        <XAxis dataKey="severity" />
        <YAxis />
        <Tooltip />
        <Bar dataKey="value" radius={[6, 6, 0, 0]} fill="#dc2626" />
      </BarChart>
    </ResponsiveContainer>
  );
}
