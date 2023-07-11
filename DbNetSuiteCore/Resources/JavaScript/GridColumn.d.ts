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
declare class GridColumn extends DbColumn {
    filter?: boolean;
    filterMode?: FilterSelectionMode;
    groupHeader?: boolean;
    download?: boolean;
    image?: boolean;
    view?: boolean;
    aggregate?: AggregateType;
    totalBreak?: boolean;
    clearDuplicateValue?: boolean;
    dataOnly?: boolean;
    constructor(properties: GridColumnResponse, unmatched?: boolean);
}
