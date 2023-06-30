interface GridColumnResponse {
    columnExpression: string;
    columnName: string;
    label: string;
    format: string;
    lookup: string;
    style: string;
    foreignKey: boolean;
    filter: boolean;
    filterMode: FilterSelectionMode;
    view: boolean;
    dataType: string;
    aggregate: AggregateType;
}
