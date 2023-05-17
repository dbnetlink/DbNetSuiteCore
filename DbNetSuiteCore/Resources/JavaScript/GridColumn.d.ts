declare enum FilterSelectionMode {
    Input = 0,
    List = 1
}
declare enum AggregateType {
    None = 0,
    Sum = 1,
    Avg = 2,
    Min = 3,
    Max = 4,
    Count = 5
}
declare class GridColumn {
    columnExpression?: string;
    columnName?: string;
    label?: string;
    format?: string;
    lookup?: string;
    style?: string;
    unmatched: boolean;
    foreignKey?: boolean;
    foreignKeyValue?: object;
    display?: boolean;
    filter?: boolean;
    filterMode?: FilterSelectionMode;
    groupHeader?: boolean;
    download?: boolean;
    image?: boolean;
    view?: boolean;
    dataType?: string;
    aggregate?: AggregateType;
    totalBreak?: boolean;
    clearDuplicateValue?: boolean;
    primaryKey?: boolean;
    index?: number;
    dataOnly?: boolean;
    constructor(properties: GridColumnResponse, unmatched?: boolean);
}
