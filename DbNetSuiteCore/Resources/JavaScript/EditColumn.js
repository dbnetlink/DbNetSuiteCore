"use strict";
class EditColumn extends DbColumn {
    constructor(properties, unmatched = false) {
        super();
        Object.keys(properties).forEach((key) => {
            if (properties[key] !== undefined)
                this[key] = properties[key];
        });
        this.unmatched = unmatched;
    }
}
