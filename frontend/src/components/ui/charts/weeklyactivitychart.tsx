import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";
import { weeklyActivity } from "@/mocks/mocks";

export function WeeklyActivityChart() {
  return (
    <div className="h-64">

    <ResponsiveContainer width="100%" height="100%">
      <LineChart data={weeklyActivity}>
        <XAxis dataKey="day" />
        <YAxis />
        <Tooltip />
        <Legend />

        <Line
          type="monotone"
          dataKey="alerts"
          stroke="#dc2626"
          strokeWidth={2}
          dot={{ r: 3 }}
        />

        <Line
          type="monotone"
          dataKey="transactions"
          stroke="#2563eb"
          strokeWidth={2}
          dot={{ r: 3 }}
        />
      </LineChart>
    </ResponsiveContainer>
    </div>
  );
}
