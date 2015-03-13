Auto mid-tier cache API sits on top of your Query Provider. System configuration sets user defined bounds for the cache.

Auto mid-tier cache automatically caches data when appropriate and responds next calls from cache. It uses popularity of a query to judge if a query needs to be cached.

For example assume you run "select .. from Customers where State = 'QLD' " and the system decides to cache it. Next time you run "select .. from Customers where state='QLD' and City='Brisbane'" query respond is calculated from the cache instead of the backend database,