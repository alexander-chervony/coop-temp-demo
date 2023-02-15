
var app = new Vue({
    el: '#app',
    data: {
        m:{},
        Errors: {},
        dismissSecs: 10,
        dismissCountDown: 0,
        loaded: false
    },
    created: function() {
        let currentObj = this;
        axios.get('/Coop/AddFirstImmovablesInit')
            .then(function (response) {
                currentObj.m = response.data;
                currentObj.loaded = true;
            })
            .catch(function (error) {
                currentObj.loaded = true;
                currentObj.Errors = error.response.data;
            });
    },

    methods: {
        addFirstImmovables: function () {
            let currentObj = this;
            axios.post('/Coop/AddFirstImmovables', currentObj.m )
                .then(function (response) {
                    currentObj.m = response.data;
                    currentObj.Errors = {};
                    currentObj.dismissCountDown = currentObj.dismissSecs;
                    currentObj.loaded = true;
                })
                .catch(function (error) {
                    currentObj.loaded = true;
                    currentObj.Errors = error.response.data;
                });
        },
        validationState: function(propertyName){
            if (!this.loaded)
                return null;
            return this.Errors[propertyName] === undefined; 
        },
        countDownChanged: function(dismissCountDown) {
            this.dismissCountDown = dismissCountDown
        },
        onDismissed: function(){
            this.dismissCountDown=0;
            window.location.href="/Coop/PleasePayEntranceFeeToProceed";
        }
    }
});


