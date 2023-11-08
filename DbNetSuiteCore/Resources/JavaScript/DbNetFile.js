"use strict";
class DbNetFile extends DbNetSuite {
    constructor(id) {
        super(id);
        this.rootFolder = "";
        this.folder = "";
        this.columns = [];
    }
    initialize() {
        if (!this.element) {
            return;
        }
        this.rootFolder = this.folder;
        this.element.empty();
        this.folderPanel = this.addPanel("folder");
        this.addLoadingPanel();
        this.callServer("page");
        this.initialised = true;
        this.fireEvent("onInitialized");
    }
    setColumnTypes(...types) {
        types.forEach(type => {
            this.columns.push(new FileColumn(type));
        });
    }
    reload() {
        this.callServer("page");
    }
    configurePage(response) {
        var _a, _b;
        (_a = this.folderPanel) === null || _a === void 0 ? void 0 : _a.html(response.html);
        (_b = this.folderPanel) === null || _b === void 0 ? void 0 : _b.find("a.folder-link").get().forEach(e => {
            $(e).on("click", (e) => this.selectFolder(e));
        });
        this.fireEvent("onPageLoaded", {});
    }
    selectFolder(event) {
        const $anchor = $(event.currentTarget);
        this.folder = $anchor.data("folder");
        this.callServer("page");
    }
    callServer(action) {
        this.post(action, this.getRequest())
            .then((response) => {
            if (response.error == false) {
                this.configurePage(response);
            }
        });
    }
    getRequest() {
        const request = this._getRequest();
        request.rootFolder = this.rootFolder;
        request.folder = this.folder;
        request.columns = this.columns.map((column) => { return column; });
        return request;
    }
}
