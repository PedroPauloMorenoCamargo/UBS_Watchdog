import { ChartCard } from "@/components/ui/charts/chartcard";
import { AdminTable } from "@/components/ui/tables/admintable";
import { usersMock, rulesMock } from "@/mocks/mocks";
import { RulesGrid } from "@/components/ui/grids/rules-grid";

import {
  Users,
  ShieldCheck,
  Settings,
  Activity,
} from "lucide-react";

import {
  Tabs,
  TabsList,
  TabsTrigger,
  TabsContent,
} from "@/components/ui/tabs";



export function AdminPage() {

  return (
    
    <div className="relative bg-cover bg-center">
      <Tabs defaultValue="users" className="w-full">
          <TabsList className="mt-5 bg-white shadow rounded-xl grid w-full grid-cols-1 sm:grid-cols-2 lg:grid-cols-2">
            <TabsTrigger value="users" 
              className="flex 
              gap-2 
              rounded-lg 
              transition
              hover:bg-slate-50
              data-[state=active]:bg-slate-200"
            >
              <Users className="h-4 w-4" />
              Users
            </TabsTrigger>

            <TabsTrigger value="rules" 
              className="flex 
              gap-2 
              rounded-lg 
              transition
              hover:bg-slate-50
              data-[state=active]:bg-slate-200"
            >
              <ShieldCheck className="h-4 w-4" />
              Rules
            </TabsTrigger>
          </TabsList>

        <div className="mt-5">
          <TabsContent value="users">
            <ChartCard title="System Users">
              <AdminTable admin={usersMock}/>
            </ChartCard>
          </TabsContent>

          <TabsContent value="rules">
            <RulesGrid
              rules={rulesMock}
              onToggleRule={(id) => console.log("toggle", id)}
              onConfigureRule={(rule) => console.log("configure", rule)}
              onDeleteRule={(id) => console.log("delete", id)}
            />
          </TabsContent>
        </div>
      </Tabs>
    </div>
  );
}

    