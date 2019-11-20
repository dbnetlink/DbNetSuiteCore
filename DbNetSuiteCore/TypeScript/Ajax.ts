/// <reference types="jquery" />
/// <reference types="notify" />

class Ajax {
    constructor() {

        /*   
        $(document).on({
            ajaxStart: function () { $("body").addClass("loading"); },
            ajaxStop: function () { $("body").removeClass("loading"); }
        });
        $.ajaxSetup({
            complete: (xhr, status) => {
                if (xhr.getResponseHeader("X-Responded-JSON") !== null && JSON.parse(xhr.getResponseHeader("X-Responded-JSON")).status === "401") {
                    this.timeout();
                }
            },
            error: (jqxhr, settings, thrownError) => {
                if (jqxhr.status.toString() === "401") { // Login has expired so tell the user and reload the page which will cause a redirect to the login page
                    this.timeout();
                } else {
                    this.ajaxError(jqxhr);
                }
            }
        });
        */
    }

    private timeout() {
        let options: BootboxAlertOptions = {} as BootboxAlertOptions;
        options.message = "Login has timed out";
        options.callback = () => this.reload();
        bootbox.alert(options); 
    }

    private ajaxError(xhr) {
        var win = window.open();
        win.document.body.innerHTML = xhr.responseText;
    }

    public callServer(handler: string, data: any, callback: any) {
        var xhr = new XMLHttpRequest();
        var _this = this;
        xhr.open('POST', `handler.dbnetgrid?handler=${handler}`);
        xhr.setRequestHeader('Content-Type', 'application/json');
        xhr.setRequestHeader("RequestVerificationToken", document.querySelector("body").getAttribute("xsrf-token"));
        xhr.onload = function () {
            if (xhr.status === 200) {
                callback(JSON.parse(xhr.responseText));
            }
            else if (xhr.status !== 200) {
                _this.ajaxError(xhr);
            }
        };
        xhr.send(JSON.stringify(data));
    }

    public reload() {
        window.location.reload();
    }

    public notifyInfo(message: string) {
        alert(message);
    }

    public notifyError(message: string) {
        alert(message);
    }

    public setAntiForgeryToken() {
        $("input[name='__RequestVerificationToken']").val($("body").attr("xsrf-token"));
    }

    public closest(element: HTMLElement, tag: string) {
        while (element.tagName !== tag.toUpperCase()) // uppercase in HTML, lower in XML
        {
            element = <HTMLElement>element.parentNode;
        }

        return element;
    }

    /*
    private notify(message: string, className: string) {
        var options = <Notify.Options>{};
        options.globalPosition = "top right";
        options.autoHide = true;
        options.className = className;
        $.notify(message, options)
    }
    */
}