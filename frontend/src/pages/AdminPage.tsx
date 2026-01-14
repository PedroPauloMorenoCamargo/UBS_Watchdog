import { ChartCard } from "@/components/ui/charts/chartcard";
import { AdminTable } from "@/components/ui/tables/admintable";
import { RulesGrid } from "@/components/ui/grids/rules-grid";
import { ConfigureRuleDialog } from "@/components/ui/dialogs/configure-rule-dialog";
import { AuditLogTable } from "@/components/ui/tables/audit-log-table";
import { Pagination } from "@/components/ui/pagination";
import { usersMock } from "@/mocks/mocks";

import {
  Users,
  ShieldCheck,
  ScrollText,
} from "lucide-react";

import {
  Tabs,
  TabsList,
  TabsTrigger,
  TabsContent,
} from "@/components/ui/tabs";

import { useApi } from "@/hooks/useApi";
import { fetchRules, patchRule, toggleRuleActive } from "@/services/rules.service";
import { fetchAuditLogs, getAuditLogDetails } from "@/services/audit-log.service";
import { fetchAllAnalysts } from "@/services/analyst.service";
import { mapDtoToRule } from "@/mappers/rule/rule.mapper";
import { AuditLogDetailsDialog } from "@/components/ui/dialogs/audit-log-details-dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useMemo, useState, useCallback } from "react";
import type { Rule } from "@/types/rules";
import { type AuditLogDto, AuditAction } from "@/types/audit-log";


export function AdminPage() {
  const { data, loading, error, refetch } = useApi({
    fetcher: fetchRules,
  });

  const [selectedRule, setSelectedRule] = useState<Rule | null>(null);
  const [isConfigureOpen, setIsConfigureOpen] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  // Audit Log Pagination & Details
  const [auditPage, setAuditPage] = useState(1);
  const auditPageSize = 20;
  const [selectedLog, setSelectedLog] = useState<AuditLogDto | null>(null);
  const [isDetailsOpen, setIsDetailsOpen] = useState(false);
  const [selectedAction, setSelectedAction] = useState<AuditAction | undefined>(undefined);

  const fetchAuditLogsCallback = useCallback(() => {
    return fetchAuditLogs({ 
      page: auditPage, 
      pageSize: auditPageSize,
      action: selectedAction
    });
  }, [auditPage, selectedAction]);

  const { data: auditLogsData, loading: auditLogsLoading, error: auditLogsError } = useApi({
    fetcher: fetchAuditLogsCallback,
    deps: [auditPage],
  });

  // Fetch Analysts for User Mapping
  const { data: analystsData } = useApi({
    fetcher: fetchAllAnalysts,
  });

  const userMap = useMemo(() => {
    if (!analystsData) return {};
    return analystsData.reduce((acc: Record<string, string>, analyst: any) => {
      acc[analyst.id] = analyst.fullName;
      return acc;
    }, {} as Record<string, string>);
  }, [analystsData]);

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

  const handleAuditLogClick = useCallback(async (log: AuditLogDto) => {
    try {
      // Fetch full details (including before/after json)
      const details = await getAuditLogDetails(log.id);
      setSelectedLog(details);
      setIsDetailsOpen(true);
    } catch (err) {
      console.error("Error fetching audit log details:", err);
      // Fallback to showing basic info if fetch fails
      setSelectedLog(log);
      setIsDetailsOpen(true);
    }
  }, []);

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
          <TabsList className="mt-5 bg-white shadow rounded-xl grid w-full grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
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

            <TabsTrigger value="audit" 
              className="flex 
              gap-2 
              rounded-lg 
              transition
              hover:bg-slate-50
              data-[state=active]:bg-slate-200"
            >
              <ScrollText className="h-4 w-4" />
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

          <TabsContent value="audit">
            <ChartCard title="Audit Log">
              {auditLogsError && <p className="text-red-500 text-center py-8">{auditLogsError}</p>}
              
              {/* Initial loading state (no data yet) */}
              {auditLogsLoading && !auditLogsData && <p className="text-center py-8">Loading audit logs...</p>}

              {/* Data display (dimmed when refreshing) */}
              {auditLogsData && (
                <div className={auditLogsLoading ? "opacity-60 transition-opacity pointer-events-none" : ""}>
                  <div className="mb-4 flex justify-end">
                    <Select
                      value={selectedAction?.toString() ?? "all"}
                      onValueChange={(val) => {
                        if (val === "all") {
                          setSelectedAction(undefined);
                        } else {
                          setSelectedAction(Number(val) as AuditAction);
                        }
                        setAuditPage(1); // Reset to first page on filter change
                      }}
                    >
                      <SelectTrigger className="w-[180px]">
                        <SelectValue placeholder="Filter by Action" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="all">All Actions</SelectItem>
                        <SelectItem value={AuditAction.Created.toString()}>Created</SelectItem>
                        <SelectItem value={AuditAction.Updated.toString()}>Updated</SelectItem>
                        <SelectItem value={AuditAction.Deleted.toString()}>Deleted</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  
                  <AuditLogTable 
                    logs={auditLogsData.items} 
                    userMap={userMap}
                    onRowClick={handleAuditLogClick}
                  />
                  <Pagination
                    currentPage={auditLogsData.page}
                    totalPages={auditLogsData.totalPages}
                    totalItems={auditLogsData.total}
                    pageSize={auditLogsData.pageSize}
                    onPageChange={setAuditPage}
                  />
                </div>
              )}
            </ChartCard>
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

      <AuditLogDetailsDialog
        open={isDetailsOpen}
        onOpenChange={setIsDetailsOpen}
        log={selectedLog}
        analystName={selectedLog?.performedByAnalystId ? userMap[selectedLog.performedByAnalystId] : undefined}
      />
    </div>
  );
}

    