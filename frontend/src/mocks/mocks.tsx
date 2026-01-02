const alertsBySeverity = [
  { severity: "High", value: 12 },
  { severity: "Medium", value: 25 },
  { severity: "Low", value: 40 },
];
const transactionsByType = [
  { name: "SWIFT", value: 45 },
  { name: "ACH", value: 30 },
  { name: "Wire", value: 18 },
  { name: "Crypto", value: 7 },
];
const COLORS = ["#dc2626", "#f59e0b", "#10b981", "#6366f1"];

const weeklyActivity = [
  { day: "Mon", transactions: 4200, alerts: 12 },
  { day: "Tue", transactions: 4100, alerts: 15 },
  { day: "Wed", transactions: 3900, alerts: 10 },
  { day: "Thu", transactions: 4300, alerts: 14 },
  { day: "Fri", transactions: 4500, alerts: 18 },
  { day: "Sat", transactions: 2800, alerts: 9 },
  { day: "Sun", transactions: 1900, alerts: 6 },
];

type Severity = "High" | "Medium" | "Low";
interface Alert {
  id: string;
  client: string;
  severity: Severity;
  rule: string;
  amount: string;
  time: string;
}
const alertsMock:Alert[] = [
  
  {
    id: "ALT-2024-1847",
    client: "Acme Corporation",
    severity: "High",
    rule: "Large Cash Deposit",
    amount: "$400,0000000085,000",
    time: "10 mins ago",
  },
  {
    id: "ALT-2024-1846",
    client: "Global Trading Ltd",
    severity: "High",
    rule: "Suspicious Pattern",
    amount: "$125,000",
    time: "25 mins ago",
  },
  {
    id: "ALT-2024-1845",
    client: "Tech Ventures Inc",
    severity: "Medium",
    rule: "Cross-Border Transaction",
    amount: "$67,500",
    time: "1 hour ago",
  },
  {
    id: "ALT-2024-1844",
    client: "Investment Group SA",
    severity: "Low",
    rule: "Tax Haven Country",
    amount: "$310,000",
    time: "2 hours ago",
  },
  {
    id: "ALT-2024-1844",
    client: "Investment Group SA",
    severity: "Low",
    rule: "Tax Haven Country",
    amount: "$310,000",
    time: "2 hours ago",
  },
  {
    id: "ALT-2024-1844",
    client: "Investment Group SA",
    severity: "Low",
    rule: "Tax Haven Country",
    amount: "$310,000",
    time: "2 hours ago",
  },
  {
    id: "ALT-2024-1844",
    client: "Investment Group SA",
    severity: "Low",
    rule: "Tax Haven Country",
    amount: "$310,000",
    time: "2 hours ago",
  },
  {
    id: "ALT-2024-1844",
    client: "Investment Group SA",
    severity: "Low",
    rule: "Tax Haven Country",
    amount: "$310,000",
    time: "2 hours ago",
  },
  {
    id: "ALT-2024-1844",
    client: "Investment Group SA",
    severity: "Low",
    rule: "Tax Haven Country",
    amount: "$310,000",
    time: "2 hours ago",
  },
];
interface Parties {
  sender: string;
  receiver: string;
}
interface Transaction {
  id: string;
  date: string;
  amount: string;
  type: string;
  parties: Parties;
  country: string;
  severity: Severity;
  status: string;
}
const transactionsMock: Transaction[] = [
  {
    id: "TX-2024-001",
    date: "2024-09-10",
    amount: "$125,000",
    type: "Wire Transfer",
    parties: {
      sender: "Acme Corporation",
      receiver: "Global Supplies Ltd",
    },
    country: "USA",
    severity: "High",
    status: "Pending",
  },
  {
    id: "TX-2024-002",
    date: "2024-09-11",
    amount: "$18,500",
    type: "Cash Deposit",
    parties: {
      sender: "John Doe",
      receiver: "First National Bank",
    },
    country: "Brazil",
    severity: "Medium",
    status: "Approved",
  },
  {
    id: "TX-2024-002",
    date: "2024-09-11",
    amount: "$1,500",
    type: "Cash Deposit",
    parties: {
      sender: "John Doe",
      receiver: "First National Bank",
    },
    country: "Brazil",
    severity: "Low",
    status: "Approved",
  },
  {
    id: "TX-2024-001",
    date: "2024-09-10",
    amount: "$125,000",
    type: "Wire Transfer",
    parties: {
      sender: "Acme Corporation",
      receiver: "Global Supplies Ltd",
    },
    country: "USA",
    severity: "High",
    status: "Pending",
  },
  {
    id: "TX-2024-002",
    date: "2024-09-11",
    amount: "$18,500",
    type: "Cash Deposit",
    parties: {
      sender: "John Doe",
      receiver: "First National Bank",
    },
    country: "Brazil",
    severity: "Medium",
    status: "Approved",
  },
  {
    id: "TX-2024-002",
    date: "2024-09-11",
    amount: "$10,500",
    type: "Cash Deposit",
    parties: {
      sender: "John Doe",
      receiver: "First National Bank",
    },
    country: "Brazil",
    severity: "Low",
    status: "Approved",
  },
  {
    id: "TX-2024-001",
    date: "2024-09-10",
    amount: "$125,000",
    type: "Wire Transfer",
    parties: {
      sender: "Acme Corporation",
      receiver: "Global Supplies Ltd",
    },
    country: "USA",
    severity: "High",
    status: "Pending",
  },
  {
    id: "TX-2024-002",
    date: "2024-09-11",
    amount: "$18,500",
    type: "Cash Deposit",
    parties: {
      sender: "John Doe",
      receiver: "First National Bank",
    },
    country: "Brazil",
    severity: "Medium",
    status: "Approved",
  },
  {
    id: "TX-2024-002",
    date: "2024-09-11",
    amount: "$10,500",
    type: "Cash Deposit",
    parties: {
      sender: "John Doe",
      receiver: "First National Bank",
    },
    country: "Brazil",
    severity: "Low",
    status: "Approved",
  },
];


export { weeklyActivity, transactionsByType, alertsBySeverity, COLORS, alertsMock, transactionsMock} ;