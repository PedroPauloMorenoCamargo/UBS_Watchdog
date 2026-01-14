export interface CaseOpenedNotification {
  caseId: string;
  clientId: string;
  accountId?: string;
  severity: number;
  openedAtUtc: string;
}
