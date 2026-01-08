export type LineConfig<T> = {
  key: keyof T
  label: string
  color: string
}

export type AdaptiveLineChartProps<T> = {
  data: T[]
  xKey: keyof T
  lines: LineConfig<T>[]
  height?: number
}
