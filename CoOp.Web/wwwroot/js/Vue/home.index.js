
var app = new Vue({
    el: '#app',
    data: {
        isBusy: false
    },
    methods: {
        myPaymentsProvider: function (ctx) {
            // Here we don't set isBusy prop, so busy state will be
            // handled by table itself
            // this.isBusy = true
            let promise = axios.get('/some/url');

            return promise.then((data) => {
                const items = data.items;
                // Here we could override the busy state, setting isBusy to false
                // this.isBusy = false
                return(items);
            }).catch(error => {
                // Here we could override the busy state, setting isBusy to false
                // this.isBusy = false
                // Returning an empty array, allows table to correctly handle
                // internal busy state in case of error
                return [];
            })
        }
    }
});

