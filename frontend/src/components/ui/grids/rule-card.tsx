import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Switch } from "@/components/ui/switch";
import { Edit, Trash2 } from "lucide-react";
import type { Rule } from "@/types/rules";
import { SeverityBadge } from "../severitybadge";

type RuleCardProps = {
  rule: Rule;
  onToggle: (id: string) => void;
  onConfigure: (rule: Rule) => void;
  onDelete: (id: string) => void;
};


export function RuleCard({
  rule,
  onToggle,
  onConfigure,
  onDelete,
}: RuleCardProps) {
  return (
    <Card className="p-5 border-gray-200">
      <div className="flex items-start justify-between mb-3">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-2">
            <h4 className="font-medium text-gray-900">{rule.name}</h4>
            <SeverityBadge severity={rule.severity}
/>
          </div>

          <p className="text-sm text-gray-600 mb-2">
            {rule.description}
          </p>

          <div className="flex items-center gap-4 text-sm text-gray-700">
            <span>Threshold: {rule.threshold}</span>
            <span>|</span>
            <span>Triggered: {rule.triggeredCount}x</span>
          </div>
        </div>

        <Switch
          checked={rule.enabled}
          onCheckedChange={() => onToggle(rule.id)}
        />
      </div>

      <div className="flex gap-2 pt-3 border-t border-gray-200">
        <Button
          variant="outline"
          size="sm"
          className="flex-1"
          onClick={() => onConfigure(rule)}
        >
          <Edit className="w-3 h-3 mr-1" />
          Configure
        </Button>

        <Button
          variant="outline"
          size="sm"
          className="flex-1 text-red-600 hover:bg-red-50"
          onClick={() => onDelete(rule.id)}
        >
          <Trash2 className="w-3 h-3 mr-1" />
          Delete
        </Button>
      </div>
    </Card>
  );
}
