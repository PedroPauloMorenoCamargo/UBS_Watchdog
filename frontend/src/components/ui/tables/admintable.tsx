import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

import { Button } from "../button";
import { Eye } from "lucide-react";
import { usersMock } from "@/mocks/mocks";
import { useRef } from "react";

interface AdminTableProps {
  admin: typeof usersMock;
}

export function AdminTable({
  admin,
}: AdminTableProps) {
  const tableRef = useRef<HTMLDivElement | null>(null);

//   useEffect(() => {
//     function handleClickOutside(event: MouseEvent) {
//       if (
//         tableRef.current &&
//         !tableRef.current.contains(event.target as Node)
//       ) {
//         onSelect(null);
//       }
//     }

//     document.addEventListener("mousedown", handleClickOutside);

//     return () => {
//       document.removeEventListener("mousedown", handleClickOutside);
//     };
//   }, [onSelect]);

  return (
    <div
      ref={tableRef}
      className="rounded-lg border bg-white max-h-[420px] overflow-y-auto"
    >
      <Table className="w-full">
        <TableHeader className="sticky top-0 z-10 bg-slate-200">
          <TableRow>
            <TableHead className="px-4 py-3">User ID</TableHead>
            <TableHead className="px-4 py-3">Name</TableHead>
            <TableHead className="px-4 py-3">Email</TableHead>
            <TableHead className="px-4 py-3 text-center">Role</TableHead>
            <TableHead className="px-4 py-3 text-center">Department</TableHead>
            <TableHead className="px-4 py-3 text-center">Status</TableHead>
            <TableHead className="px-4 py-3 text-left">Last Login</TableHead>
            <TableHead className="px-4 py-3 text-center">Actions</TableHead>
          </TableRow>
        </TableHeader>

        <TableBody>
          {admin.map((user) => {
            // const selected = user.id === selectedId;

            return (
              <TableRow
                // key={`${user.id}-${index}`}
                // onClick={() =>
                //   onSelect(selected ? null : user.id)
                // }
                // className={`
                //   cursor-pointer
                //   ${selected ? "bg-slate-100" : "hover:bg-slate-50"}
                // `}
                // aria-selected={selected}
              >
                <TableCell className="px-4 py-3 text-sm font-medium text-slate-700">
                  {user.id}
                </TableCell>

                <TableCell className="px-4 py-3 text-sm font-medium text-slate-700">
                  {user.name}
                </TableCell>

                <TableCell className="px-4 py-3 text-sm text-slate-600">
                  {user.email}
                </TableCell>

                <TableCell className="px-4 py-3 text-sm text-center">
                  {user.role}
                </TableCell>

                <TableCell className="px-4 py-3 text-sm text-center">
                 {user.department}
                </TableCell>

                <TableCell className="px-4 py-3 text-sm text-center">
                  <span
                    className={`inline-flex items-center justify-center min-w-[72px] rounded-md border px-2 py-1 text-xs font-medium
                      ${
                        user.status === "active"
                          ? "bg-green-100 text-green-800 border-green-300"
                          : "bg-gray-100 text-gray-700 border-gray-300"
                      }
                    `}
                  >
                    {user.status === "active" ? "Active" : "Inactive"}
                  </span>
                </TableCell>
                
                <TableCell className="px-4 py-3 text-sm text-left text-slate-600">
                  {user.lastLogin}
                </TableCell>

                <TableCell className="px-4 py-3 text-sm text-left text-slate-600">
                   <Button
                      variant="ghost"
                      size="sm"
                      // onClick={() => setSelectedClient(user)} TODO: modal para tratar user
                      className="text-[#e60028] hover:text-[#b8001f] hover:bg-red-50"
                    >
                      <Eye className="w-4 h-4 mr-1" />
                      View
                    </Button>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </div>
  );
}
