import { RuleCard } from "./rule-card";
import type { Rule } from "@/types/rules";

type RulesGridProps = {
  rules: Rule[];
  onToggleRule: (id: string) => void;
  onConfigureRule: (rule: Rule) => void;
  onDeleteRule: (id: string) => void;
};

export function RulesGrid({
  rules,
  onToggleRule,
  onConfigureRule,
  onDeleteRule,
}: RulesGridProps) {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
      {rules.map((rule) => (
        <RuleCard
          key={rule.id}
          rule={rule}
          onToggle={onToggleRule}
          onConfigure={onConfigureRule}
          onDelete={onDeleteRule}
        />
      ))}
    </div>
  );
}
