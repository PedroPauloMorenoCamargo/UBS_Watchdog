import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { useEffect, useState } from "react";
import { fetchClientDetail } from "@/services/reports.service";
import { fetchClientAccounts } from "@/services/accounts.service";
import type { ClientDetailDto } from "@/types/Reports/report";
import type { AccountResponseDto } from "@/types/Accounts/account";
import { accountTypeMap, accountStatusMap, AccountStatus } from "@/types/Accounts/account";
import { SeverityBadge } from "@/components/ui/severitybadge";
import { KYCStatusBadge } from "@/components/ui/kycstatusbadge";
import { useNavigate } from "react-router-dom";
import { Loader2, ChevronDown, ChevronUp, Plus, Wallet, Key, Trash2 } from "lucide-react";
import { CreateAccountDialog } from "./create-account-dialog";
import { CreateIdentifierDialog } from "./create-identifier-dialog";
import { ConfirmDialog } from "./confirm-dialog";
import { fetchAccountIdentifiers, deleteAccountIdentifier } from "@/services/accountIdentifiers.service";
import type { AccountIdentifierDto } from "@/types/AccountIdentifiers/accountIdentifier";
import { identifierTypeMap } from "@/types/AccountIdentifiers/accountIdentifier";

interface ClientDetailsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  clientId: string;
}

const legalTypeMap: Record<number, string> = {
  0: "Individual",
  1: "Corporate",
};

export function ClientDetailsDialog({
  open,
  onOpenChange,
  clientId,
}: ClientDetailsDialogProps) {
  const [client, setClient] = useState<ClientDetailDto | null>(null);
  const [accounts, setAccounts] = useState<AccountResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [accountsLoading, setAccountsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [accountsExpanded, setAccountsExpanded] = useState(false);
  const [createAccountOpen, setCreateAccountOpen] = useState(false);
  const [createIdentifierOpen, setCreateIdentifierOpen] = useState(false);
  const [selectedAccountId, setSelectedAccountId] = useState<string | null>(null);
  const [accountIdentifiers, setAccountIdentifiers] = useState<Record<string, AccountIdentifierDto[]>>({});
  const [expandedAccounts, setExpandedAccounts] = useState<Set<string>>(new Set());
  const [loadingIdentifiers, setLoadingIdentifiers] = useState<Set<string>>(new Set());
  const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);
  const [identifierToDelete, setIdentifierToDelete] = useState<{ id: string; accountId: string } | null>(null);
  const navigate = useNavigate();

  const loadAccounts = async () => {
    if (!clientId) return;
    setAccountsLoading(true);
    try {
      const data = await fetchClientAccounts(clientId);
      setAccounts(data);
    } catch (err) {
      console.error("Failed to load accounts:", err);
    } finally {
      setAccountsLoading(false);
    }
  };

  const loadIdentifiers = async (accountId: string) => {
    setLoadingIdentifiers((prev) => new Set(prev).add(accountId));
    try {
      const identifiers = await fetchAccountIdentifiers(accountId);
      setAccountIdentifiers((prev) => ({ ...prev, [accountId]: identifiers }));
    } catch (err) {
      console.error("Failed to load identifiers:", err);
    } finally {
      setLoadingIdentifiers((prev) => {
        const newSet = new Set(prev);
        newSet.delete(accountId);
        return newSet;
      });
    }
  };

  const handleDeleteIdentifier = async (identifierId: string, accountId: string) => {
    setIdentifierToDelete({ id: identifierId, accountId });
    setConfirmDeleteOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!identifierToDelete) return;

    try {
      await deleteAccountIdentifier(identifierToDelete.id);
      // Reload identifiers for this account
      await loadIdentifiers(identifierToDelete.accountId);
    } catch (err) {
      console.error("Failed to delete identifier:", err);
      alert("Failed to delete identifier");
    } finally {
      setConfirmDeleteOpen(false);
      setIdentifierToDelete(null);
    }
  };

  const toggleAccountExpanded = (accountId: string) => {
    setExpandedAccounts((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(accountId)) {
        newSet.delete(accountId);
      } else {
        newSet.add(accountId);
        // Load identifiers when expanding
        if (!accountIdentifiers[accountId]) {
          loadIdentifiers(accountId);
        }
      }
      return newSet;
    });
  };

  useEffect(() => {
    if (open && clientId) {
      setLoading(true);
      setError(null);
      setAccountsExpanded(false);

      fetchClientDetail(clientId)
        .then((data) => {
          setClient(data);
        })
        .catch((err) => {
          console.error(err);
          setError("Failed to load client details");
        })
        .finally(() => {
          setLoading(false);
        });

      // Load accounts when dialog opens
      loadAccounts();
    }
  }, [open, clientId]);

  const handleViewTransactions = () => {
    onOpenChange(false);
    navigate(`/transactions?clientId=${clientId}`);
  };

  const handleViewReports = () => {
    onOpenChange(false);
    navigate(`/reports/client/${clientId}`);
  };

  const handleViewAlerts = () => {
    onOpenChange(false);
    navigate(`/alerts?clientId=${clientId}`);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            Client Details - {client?.name || "Loading..."}
          </DialogTitle>
        </DialogHeader>

        {loading && (
          <div className="flex items-center justify-center py-8">
            <Loader2 className="h-8 w-8 animate-spin text-slate-400" />
          </div>
        )}

        {error && (
          <div className="text-red-500 text-center py-4">{error}</div>
        )}

        {!loading && !error && client && (
          <div className="space-y-6">
            {/* Client Information */}
            <div>
              <h3 className="text-sm font-semibold text-slate-700 mb-3">
                Client Information
              </h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-xs text-slate-500">Client ID</p>
                  <p className="text-sm font-medium text-slate-900">
                    {client.id}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-slate-500">Client Name</p>
                  <p className="text-sm font-medium text-slate-900">
                    {client.name}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-slate-500">Country</p>
                  <p className="text-sm font-medium text-slate-900">
                    {client.countryCode}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-slate-500">Client Type</p>
                  <p className="text-sm font-medium text-slate-900">
                    {legalTypeMap[client.legalType] || "Unknown"}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-slate-500">Phone</p>
                  <p className="text-sm font-medium text-slate-900">
                    {client.contactNumber}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-slate-500">Registration Date</p>
                  <p className="text-sm font-medium text-slate-900">
                    {new Date(client.createdAtUtc).toLocaleDateString()}
                  </p>
                </div>
              </div>
            </div>

            {/* Address */}
            {client.addressJson && (
              <div>
                <h3 className="text-sm font-semibold text-slate-700 mb-3">
                  Address
                </h3>
                <div className="text-sm text-slate-900">
                  {client.addressJson.street && (
                    <p>{client.addressJson.street}</p>
                  )}
                  {(client.addressJson.city || client.addressJson.state) && (
                    <p>
                      {client.addressJson.city}
                      {client.addressJson.city && client.addressJson.state && ", "}
                      {client.addressJson.state}
                    </p>
                  )}
                  {client.addressJson.zipCode && (
                    <p>{client.addressJson.zipCode}</p>
                  )}
                </div>
              </div>
            )}

            {/* Account Information */}
            <div>
              <h3 className="text-sm font-semibold text-slate-700 mb-3">
                Accounts
              </h3>
              
              {/* Accounts Accordion */}
              <div className="border rounded-lg overflow-hidden">
                <button
                  type="button"
                  onClick={() => setAccountsExpanded(!accountsExpanded)}
                  className="w-full flex items-center justify-between p-3 hover:bg-slate-50 transition-colors bg-white"
                >
                  <div className="flex items-center gap-2">
                    <Wallet className="h-4 w-4 text-slate-500" />
                    <span className="text-sm font-medium text-slate-700">
                      {accounts.length === 0 
                        ? "No accounts yet" 
                        : `${accounts.length} account${accounts.length > 1 ? "s" : ""}`}
                    </span>
                  </div>
                  <div className="flex items-center gap-2">
                    {accountsLoading && (
                      <Loader2 className="h-4 w-4 animate-spin text-slate-400" />
                    )}
                    {accountsExpanded ? (
                      <ChevronUp className="h-4 w-4 text-slate-500" />
                    ) : (
                      <ChevronDown className="h-4 w-4 text-slate-500" />
                    )}
                  </div>
                </button>
                
                {accountsExpanded && (
                  <div className="border-t bg-slate-50">
                    {accounts.length > 0 ? (
                      <div className="divide-y max-h-60 overflow-y-auto bg-white">
                        {accounts.map((account) => {
                          const isExpanded = expandedAccounts.has(account.id);
                          const identifiers = accountIdentifiers[account.id] || [];
                          const isLoadingIds = loadingIdentifiers.has(account.id);
                          
                          return (
                          <div
                            key={account.id}
                            className="hover:bg-slate-50 transition-colors"
                          >
                            <div className="p-3">
                              <div className="flex items-center justify-between mb-1">
                                <button
                                  onClick={() => toggleAccountExpanded(account.id)}
                                  className="flex items-center gap-2 flex-1 text-left"
                                >
                                  {isExpanded ? (
                                    <ChevronUp className="h-3 w-3 text-slate-400" />
                                  ) : (
                                    <ChevronDown className="h-3 w-3 text-slate-400" />
                                  )}
                                  <span className="text-sm font-medium text-slate-900">
                                    {account.accountIdentifier}
                                  </span>
                                </button>
                                <span
                                  className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                                    account.status === AccountStatus.Active
                                      ? "bg-green-100 text-green-700"
                                      : account.status === AccountStatus.Blocked
                                      ? "bg-red-100 text-red-700"
                                      : "bg-gray-100 text-gray-700"
                                  }`}
                                >
                                  {accountStatusMap[account.status]}
                                </span>
                              </div>
                              <div className="flex items-center gap-4 text-xs text-slate-500 mb-2 ml-5">
                                <span className="flex items-center gap-1">
                                  <span className="font-medium">Type:</span>
                                  {accountTypeMap[account.accountType]}
                                </span>
                                <span className="flex items-center gap-1">
                                  <span className="font-medium">Currency:</span>
                                  {account.currencyCode}
                                </span>
                                <span className="flex items-center gap-1">
                                  <span className="font-medium">Country:</span>
                                  {account.countryCode}
                                </span>
                              </div>

                              {/* Identifiers Section */}
                              {isExpanded && (
                                <div className="ml-5 mt-2 space-y-2">
                                  {isLoadingIds ? (
                                    <div className="flex items-center justify-center py-2">
                                      <Loader2 className="h-4 w-4 animate-spin text-slate-400" />
                                    </div>
                                  ) : identifiers.length > 0 ? (
                                    <div className="space-y-1">
                                      <p className="text-xs font-medium text-slate-600 mb-1">Identifiers:</p>
                                      {identifiers.map((identifier) => (
                                        <div
                                          key={identifier.id}
                                          className="flex items-center justify-between bg-slate-50 p-2 rounded text-xs"
                                        >
                                          <div>
                                            <span className="font-medium text-slate-700">
                                              {identifierTypeMap[identifier.identifierType]}:
                                            </span>{" "}
                                            <span className="text-slate-900">{identifier.identifierValue}</span>
                                            {identifier.issuedCountryCode && (
                                              <span className="text-slate-500 ml-2">({identifier.issuedCountryCode})</span>
                                            )}
                                          </div>
                                          <Button
                                            variant="ghost"
                                            size="sm"
                                            onClick={() => handleDeleteIdentifier(identifier.id, account.id)}
                                            className="h-6 w-6 p-0 text-red-500 hover:text-red-700 hover:bg-red-50"
                                          >
                                            <Trash2 className="h-3 w-3" />
                                          </Button>
                                        </div>
                                      ))}
                                    </div>
                                  ) : (
                                    <p className="text-xs text-slate-500 py-1">No identifiers yet</p>
                                  )}
                                </div>
                              )}

                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() => {
                                  setSelectedAccountId(account.id);
                                  setCreateIdentifierOpen(true);
                                }}
                                className="w-full h-7 text-xs mt-2"
                              >
                                <Key className="h-3 w-3 mr-1" />
                                Add Identifier
                              </Button>
                            </div>
                          </div>
                        );
                        })}
                      </div>
                    ) : (
                      <div className="p-4 text-center text-sm text-slate-500">
                        No accounts created yet
                      </div>
                    )}
                    
                    {/* Create Account Button - Always visible when expanded */}
                    <div className="p-3 border-t bg-slate-50">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setCreateAccountOpen(true)}
                        className="w-full"
                      >
                        <Plus className="h-4 w-4 mr-2" />
                        Create New Account
                      </Button>
                    </div>
                  </div>
                )}
              </div>
            </div>

            {/* Risk & Compliance */}
            <div>
              <h3 className="text-sm font-semibold text-slate-700 mb-3">
                Risk & Compliance
              </h3>
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <p className="text-xs text-slate-500 mb-1">Risk Level</p>
                  <SeverityBadge
                    severity={
                      client.riskLevel === 2
                        ? "high"
                        : client.riskLevel === 1
                        ? "medium"
                        : "low"
                    }
                  />
                </div>
                <div>
                  <p className="text-xs text-slate-500 mb-1">KYC Status</p>
                  <KYCStatusBadge
                    kyc={
                      client.kycStatus === 1
                        ? "Verified"
                        : client.kycStatus === 2
                        ? "Expired"
                        : client.kycStatus === 3
                        ? "Rejected"
                        : "Pending"
                    }
                  />
                </div>
                <div>
                  <p className="text-xs text-slate-500">Total Alerts</p>
                  <p className="text-sm font-medium text-slate-900">
                    {client.totalCases}
                  </p>
                </div>
              </div>
            </div>

            {/* Activity Summary */}
            <div>
              <h3 className="text-sm font-semibold text-slate-700 mb-3">
                Activity Summary
              </h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-xs text-slate-500">Total Transactions</p>
                  <p className="text-sm font-medium text-slate-900">
                    {client.totalTransactions}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-slate-500">Last Activity</p>
                  <p className="text-sm font-medium text-slate-900">
                    {new Date(client.updatedAtUtc).toLocaleDateString()}
                  </p>
                </div>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex gap-3 pt-4 border-t">
              <Button
                onClick={handleViewTransactions}
                variant="outline"
                className="flex-1"
              >
                View Transactions
              </Button>
              <Button
                onClick={handleViewAlerts}
                variant="outline"
                className="flex-1"
              >
                View Alerts
              </Button>
              <Button
                onClick={handleViewReports}
                variant="outline"
                className="flex-1"
              >
                View Reports
              </Button>
            </div>
          </div>
        )}

        {/* Create Account Dialog */}
        <CreateAccountDialog
          open={createAccountOpen}
          onOpenChange={setCreateAccountOpen}
          clientId={clientId}
          clientCountryCode={client?.countryCode}
          onSuccess={loadAccounts}
        />

        {/* Create Identifier Dialog */}
        {selectedAccountId && (
          <CreateIdentifierDialog
            open={createIdentifierOpen}
            onOpenChange={setCreateIdentifierOpen}
            accountId={selectedAccountId}
            onSuccess={() => {
              // Reload identifiers for this account
              loadIdentifiers(selectedAccountId);
            }}
          />
        )}

        {/* Confirm Delete Dialog */}
        <ConfirmDialog
          open={confirmDeleteOpen}
          onOpenChange={setConfirmDeleteOpen}
          title="Delete Identifier"
          description="Are you sure you want to delete this identifier? This action cannot be undone."
          confirmLabel="Delete"
          cancelLabel="Cancel"
          variant="destructive"
          onConfirm={handleConfirmDelete}
        />
      </DialogContent>
    </Dialog>
  );
}
