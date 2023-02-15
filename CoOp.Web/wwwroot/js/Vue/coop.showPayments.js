
var app = new Vue({
    el: '#app',
    data: {
        m: {
            Immovables:[]
        },
        Errors: {},
        loaded: false
    },
   
    methods: {
        showPayments: function (action) {
            let currentObj = this;
            axios.post(action,  currentObj.m )
                .then(function (response) {
                    currentObj.loaded = true;
                    console.log(response.data);
                    currentObj.m.Immovables = response.data;
                })
                .catch(function (error) {
                    currentObj.loaded = true;
                    currentObj.Errors = error.response.data;
                });
        }
    },
    created: function() {
        this.showPayments('/Coop/ShowPayments');
    }
});


