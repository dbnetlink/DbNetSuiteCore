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

class GridColumn extends DbColumn {
    filter?: boolean;
    filterMode?: FilterSelectionMode;
    groupHeader?: boolean;
    view?: boolean;
    aggregate?: AggregateType
    totalBreak?: boolean;
    clearDuplicateValue?: boolean;
    dataOnly?: boolean;

    constructor(properties: GridColumnResponse | undefined = undefined, unmatched = false) {
        super();
        this.unmatched = unmatched;
        if (properties) {
            this.assignProperties(properties);
        }
    }

    public assignProperties(properties: GridColumnResponse) {
        Object.keys(properties).forEach((key) => {
            if (properties[key as keyof GridColumnResponse] !== undefined)
                this[key as keyof GridColumnResponse] = properties[key as keyof GridColumnResponse] as any;
        });
    }
}