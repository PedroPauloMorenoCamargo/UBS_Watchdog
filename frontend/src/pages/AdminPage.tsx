import { ChartCard } from "@/components/ui/charts/chartcard";
import { AdminTable } from "@/components/ui/tables/admintable";
import { RulesGrid } from "@/components/ui/grids/rules-grid";
import { ConfigureRuleDialog } from "@/components/ui/dialogs/configure-rule-dialog";
import { usersMock } from "@/mocks/mocks";

import {
  Users,
  ShieldCheck,
} from "lucide-react";

import {
  Tabs,
  TabsList,
  TabsTrigger,
  TabsContent,
} from "@/components/ui/tabs";

import { useApi } from "@/hooks/useApi";
import { fetchRules, patchRule, toggleRuleActive } from "@/services/rules.service";
import { mapDtoToRule } from "@/mappers/rule/rule.mapper";
import { useMemo, useState, useCallback } from "react";
import type { Rule } from "@/types/rules";


export function AdminPage() {
  const { data, loading, error, refetch } = useApi({
    fetcher: fetchRules,
  });

  const [selectedRule, setSelectedRule] = useState<Rule | null>(null);
  const [isConfigureOpen, setIsConfigureOpen] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  const rules = useMemo(() => {
    if (!data) return [];
    // Sort by code to keep rules in a fixed position
    return data.items
      .map(mapDtoToRule)
      .sort((a, b) => a.code.localeCompare(b.code));
  }, [data]);

  const handleToggleRule = useCallback(async (id: string) => {
    const rule = rules.find((r) => r.id === id);
    if (!rule) return;

    try {
      await toggleRuleActive(id, !rule.enabled);
      refetch();
    } catch (err) {
      console.error("Error toggling rule:", err);
      alert("Error toggling rule status");
    }
  }, [rules, refetch]);

  const handleConfigureRule = useCallback((rule: Rule) => {
    setSelectedRule(rule);
    setIsConfigureOpen(true);
  }, []);

  const handleSaveRule = useCallback(async (id: string, data: {
    name?: string;
    isActive?: boolean;
    severity?: "Low" | "Medium" | "High" | "Critical";
    scope?: string;
    parameters?: Record<string, unknown>;
  }) => {
    // Remove undefined fields
    const cleanData = Object.fromEntries(
      Object.entries(data).filter(([, v]) => v !== undefined)
    );

    if (Object.keys(cleanData).length === 0) {
      setIsConfigureOpen(false);
      return;
    }

    setIsSaving(true);
    try {
      await patchRule(id, cleanData);
      setIsConfigureOpen(false);
      refetch();
    } catch (err) {
      console.error("Error saving rule:", err);
      alert("Error saving rule configuration");
    } finally {
      setIsSaving(false);
    }
  }, [refetch]);

  const handleDeleteRule = useCallback((id: string) => {
    // To delete a rule, you can disable it or implement a DELETE endpoint
    console.log("Delete rule:", id);
    alert("Delete functionality not implemented. Use the toggle to disable the rule.");
  }, []);

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
            {loading && <p>Loading rules...</p>}
            {error && <p className="text-red-500">{error}</p>}

            {!loading && !error && (
              <RulesGrid
                rules={rules}
                onToggleRule={handleToggleRule}
                onConfigureRule={handleConfigureRule}
                onDeleteRule={handleDeleteRule}
              />
            )}
          </TabsContent>
        </div>
      </Tabs>

      <ConfigureRuleDialog
        open={isConfigureOpen}
        rule={selectedRule}
        onOpenChange={setIsConfigureOpen}
        onSave={handleSaveRule}
        loading={isSaving}
      />
    </div>
  );
}

    