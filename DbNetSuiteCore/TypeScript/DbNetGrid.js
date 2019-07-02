/// <reference types="jquery" />
/// <reference types="bootstrap" />
/// <reference types="bootbox" />
/// <reference path="Ajax.ts" />
/// <reference path="Interfaces.ts" />
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var DbNetGrid = /** @class */ (function (_super) {
    __extends(DbNetGrid, _super);
    function DbNetGrid(configuration) {
        var _this = _super.call(this) || this;
        _this.configuration = configuration;
        _this.init();
        return _this;
    }
    DbNetGrid.prototype.init = function () {
        var _this = this;
        this.callServer("Init", this.configuration, function (response) { _this.initCallback(response); });
    };
    DbNetGrid.prototype.initCallback = function (response) {
        this.$container = $("#" + this.configuration.id);
        this.$container.html(response.html.toolbar);
        this.$container.find(".table-container").html(response.html.page);
    };
    return DbNetGrid;
}(Ajax));
//# sourceMappingURL=DbNetGrid.js.map