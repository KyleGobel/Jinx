angular.module("api", [])
    .factory('databaseApi', function($http, $q) {
        return {
            insert: insert,
            update: update,
            remove: remove,
            getAll: getAll
        };

        function getAll() {
            var d = $q.defer();
            $http.get("/database")
                .then(function(results) {
                    d.resolve(results.data);
                }, function(error) {
                    d.reject(error.data);
                });
            return d.promise;
        }
        function insert(databaseModel) {
            var d = $q.defer();

            $http.post("/database", databaseModel)
                .then(function(response) {
                    d.resolve(response);
                }, function(error) {
                    d.reject(error.data);
                });

            return d.promise;
        }

        function update(databaseModel) {
            var d = $q.defer();

            $http.put("/database", databaseModel)
                .then(function(response) {
                    d.resolve(response);
                }, function(error) {
                    d.reject(error.data);
                });

            return d.promise;
        }

        function remove(id) {
            var d = $q.defer();
            $http.delete("/database", { id: id })
                .then(function(response) {
                    d.resolve(response);
                }, function(error) {
                    d.reject(error.data);
                });

            return d.promise;
        }
    })
.factory('jobApi', function($http, $q) {

    return {
        insert: insert,
        buildModel: buildModel
    };
    function insert(jobModel) {
        var d = $q.defer();
        $http.post("/job", jobModel)
            .then(function(response) {
                d.resolve(response);
            }, function(error) {
                d.reject(error.data);
            });
        return d.promise;
    }

    function buildModel(databaseId, sourceSql) {
        var d = $q.defer();

        $http.post("/job/buildModel", {
            databaseId: databaseId,
            sql: sourceSql
        }).then(function(response) {
            d.resolve(response.data);
        }, function(error) {
            d.reject(error.data);
        });
        return d.promise;
    }

});