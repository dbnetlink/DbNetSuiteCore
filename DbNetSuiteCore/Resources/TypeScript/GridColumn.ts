enum FilterSelectionMode {
    Input,
    List
}

enum AggregateType {
    None,
    Sum,
    Avg,
    Min,
    Max,
    Count
}

class GridColumn {
    columnExpression?: string;
    columnName?: string;
    columnKey?: string;
    label?: string;
    format?: string;
    lookup?: string;
    style?: string;
    unmatched: boolean = false;
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
    aggregate?: AggregateType
    totalBreak?: boolean;
    clearDuplicateValue?: boolean;
    primaryKey?: boolean;
    index?: number;
    dataOnly?: boolean;

    constructor(properties: GridColumnResponse, unmatched: boolean = false) {
        Object.keys(properties).forEach((key) => {
            if (properties[key as keyof GridColumnResponse] !== undefined)
                this[key as keyof GridColumnResponse] = properties[key as keyof GridColumnResponse] as any;
        });

        this.unmatched = unmatched;
    }
}