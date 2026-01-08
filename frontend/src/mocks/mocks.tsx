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
const weeklyAlertsBySeverity = [
  { day: "Mon", high: 5, medium: 4, low: 3 },
  { day: "Tue", high: 6, medium: 5, low: 4 },
  { day: "Wed", high: 4, medium: 3, low: 3 },
  { day: "Thu", high: 5, medium: 6, low: 3 },
  { day: "Fri", high: 7, medium: 6, low: 5 },
  { day: "Sat", high: 3, medium: 4, low: 2 },
  { day: "Sun", high: 2, medium: 3, low: 1 },
];

type Severity = "High" | "Medium" | "Low";
type KYC = "Verified" | "Expired" | "Pending";
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
    id: "TX-2024-003",
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
    id: "TX-2024-004",
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
    id: "TX-2024-005",
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
    id: "TX-2024-006",
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
    id: "TX-2024-007",
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
    id: "TX-2024-008",
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
    id: "TX-2024-009",
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

interface Client {
  id: string,
  name: string,
  country: string,
  risk:Severity,
  kyc:KYC,
  alerts:number,
  balance:number,
  lastActivity:string,
}

const clientsMock: Client[] = [
  {
     id: "1",
    name: "João Silva ",
    country: "Brazil",
    risk: "Medium",
    kyc: "Verified",
    alerts: 2,
    balance: 15432.75,
    lastActivity: "2026-01-05T14:32:00Z",
  },
  {
     id: "2",
    name: "Maria Silva ",
    country: "Brazil",
    risk: "Low",
    kyc: "Pending",
    alerts: 2,
    balance: 15432.75,
    lastActivity: "2026-01-05T14:32:00Z",
  },
  {
     id: "3",
    name: "Pedro Silva ",
    country: "Brazil",
    risk: "High",
    kyc: "Expired",
    alerts: 25,
    balance: 1543662.75,
    lastActivity: "2026-01-05T14:32:00Z",
  }

]

interface Report {
  name: string,
  date: string,
  by: string,
  status: string,
}

const reportsMock: Report[]=[
   {
    name: "Monthly Compliance Report",
    date: "2025-12-01",
    by: "John Doe",
    status: "ready",
  },
  {
    name: "KYC Risk Assessment",
    date: "2025-12-05",
    by: "Maria Silva",
    status: "pending",
  },
  {
    name: "Transaction Monitoring Summary",
    date: "2025-11-28",
    by: "Carlos Mendes",
    status: "ready",
  },
  {
    name: "Sanctions Screening Report",
    date: "2025-12-10",
    by: "Ana Costa",
    status: "pending",
  },
  {
    name: "PEP Analysis Report",
    date: "2025-12-12",
    by: "Lucas Pereira",
    status: "failed",
  },
]


export { weeklyActivity,weeklyAlertsBySeverity, transactionsByType, alertsBySeverity, COLORS, alertsMock, transactionsMock, clientsMock, reportsMock};