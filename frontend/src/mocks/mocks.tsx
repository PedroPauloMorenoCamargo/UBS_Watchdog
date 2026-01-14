interface User {
  id: string,
  name: string,
  email: string,
  role: string,
  department: string,
  status: string,
  lastLogin: string,
}

const usersMock: User[]=[
  {
    id: "u-001",
    name: "Ana Silva",
    email: "ana.silva@empresa.com",
    role: "Admin",
    department: "Compliance",
    status: "active",
    lastLogin: "2026-01-08T14:32:00Z",
  },
  {
    id: "u-002",
    name: "Bruno Costa",
    email: "bruno.costa@empresa.com",
    role: "Analyst",
    department: "Risk",
    status: "active",
    lastLogin: "2026-01-07T09:15:00Z",
  },
  {
    id: "u-003",
    name: "Carla Mendes",
    email: "carla.mendes@empresa.com",
    role: "Manager",
    department: "Operations",
    status: "inactive",
    lastLogin: "2025-12-22T18:40:00Z",
  },
] 


export { usersMock};