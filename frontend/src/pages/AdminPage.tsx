import { ChartCard } from "@/components/ui/charts/chartcard";
import { AdminTable } from "@/components/ui/tables/admintable";
import { usersMock } from "@/mocks/mocks";

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
          <TabsList className="mt-5 bg-white shadow rounded-xl grid w-full grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
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

            <TabsTrigger value="settings" 
              className="flex 
              gap-2 
              rounded-lg 
              transition
              hover:bg-slate-50
              data-[state=active]:bg-slate-200"
            >
              <Settings className="h-4 w-4" />
              Settings
            </TabsTrigger>

            <TabsTrigger value="audit" 
              className="flex 
              gap-2 
              rounded-lg 
              transition
              hover:bg-slate-50
              data-[state=active]:bg-slate-200"
            >
              <Activity className="h-4 w-4" />
              Audit Log
            </TabsTrigger>
          </TabsList>

        <div className="mt-5">
          <TabsContent value="users">
            <ChartCard title="System Users">
              <AdminTable admin={usersMock}/>
            </ChartCard>
          </TabsContent>

          <TabsContent value="rules">
            {/* TODO: RULES CARDS */}
          </TabsContent>

          <TabsContent value="settings">
            {/* TODO: SETTINGS OPTIONS */}
          </TabsContent>

          <TabsContent value="audit">
            {/* TODO: HISTORY TABLE */}
          </TabsContent>
        </div>
      </Tabs>
    </div>
  );
}

    