"use strict";
var FilterSelectionMode;
(function (FilterSelectionMode) {
    FilterSelectionMode[FilterSelectionMode["Input"] = 0] = "Input";
    FilterSelectionMode[FilterSelectionMode["List"] = 1] = "List";
})(FilterSelectionMode || (FilterSelectionMode = {}));
var AggregateType;
(function (AggregateType) {
    AggregateType[AggregateType["None"] = 0] = "None";
    AggregateType[AggregateType["Sum"] = 1] = "Sum";
    AggregateType[AggregateType["Avg"] = 2] = "Avg";
    AggregateType[AggregateType["Min"] = 3] = "Min";
    AggregateType[AggregateType["Max"] = 4] = "Max";
    AggregateType[AggregateType["Count"] = 5] = "Count";
})(AggregateType || (AggregateType = {}));
class GridColumn extends DbColumn {
    constructor(properties, unmatched = false) {
        super();
        Object.keys(properties).forEach((key) => {
            if (properties[key] !== undefined)
                this[key] = properties[key];
        });
        this.unmatched = unmatched;
    }
}
